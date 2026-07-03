# Personal Finance Manager — Completion Roadmap

> Architect's plan to take this solution from its current state to a working
> **Web + API** product, then a **WPF Desktop** client, with the architecture
> deliberately prepared for the later **full suite** (Mobile, cloud, advanced).

**Committed scope (now):** Phases 0–5 (Web+API MVP → Quality → Desktop).
**Planned next (after Phase 5 passes dev tests):** Phase 6+ (Mobile, Azure/CI-CD, advanced).

## Progress

| Phase | Status |
|---|---|
| 0 — Stabilize foundation | ✅ Completed 2026-06-24 |
| 1 — Application layer + custom JWT auth | ✅ Completed 2026-06-24 |
| 2 — API domain controllers | ✅ Completed 2026-07-03 |
| 3 — Web end-to-end | ✅ Completed 2026-07-03 |
| 4 — Tests + CI | ✅ Completed 2026-07-03 |
| 5 — Desktop (WPF) + sync | ⏳ Next |
| 6+ — Full suite | ⬜ Planned |

> This file is kept up to date as each phase completes and is committed to the repo.

---

## 1. Decisions log

| Decision | Choice | Rationale |
|---|---|---|
| Authentication | **Custom JWT issuance** (BCrypt + `System.IdentityModel.Tokens.Jwt`) | Matches existing Web `JwtAuthStateProvider` / `AuthTokenHandler`; less ceremony; exposes the mechanics for learning. |
| Orchestration | **New `PersonalFinanceManager.Application` layer** | Multi-entity, transactional use-cases need a home; keeps controllers thin; high learning value. |
| API contracts | **DTOs in Application/API, never expose domain entities** | Domain entities have private setters + internal constructors and can't be model-bound. |
| Schema management | **EF Core migrations** (replace `EnsureCreated`/reset-on-startup) | Safe, incremental, production-realistic. |
| Scope | MVP + Desktop now; full suite designed-for, built later | Working vertical slice first; breadth after dev tests pass. |

---

## 2. Current state snapshot

**Done & solid:** Core domain (rich aggregate `User`, entities with invariants, value objects, enums, domain exceptions); Infrastructure (EF `AppDBContext` with soft-delete filters + audit stamping + hard→soft delete, per-entity configs, generic + specific repositories, `UnitOfWork` with explicit transactions); API cross-cutting (`GlobalExceptionHandlerMiddleware`, Swagger, CORS, JSON enum-as-string).

**Missing / broken:**
- No application/orchestration layer.
- No authentication (no password hashing, no JWT issuance, no `api/auth/*`, no auth scheme in the pipeline).
- API has only `HealthController`; all domain controllers absent.
- Web does not build (deleted `Create*Model.cs` still referenced; `ApiResult` vs `ApiResponse` mismatch; `AuthService` calls an `IApiClient.PostAsync<TResponse>` overload that doesn't exist).
- DB wiped on every API startup; no migrations; no tests; Web targets net6 vs net8 elsewhere.

---

## 3. Target architecture

```
PersonalFinanceManager.Web   (Blazor Server)
        │  HTTP + Bearer JWT
        ▼
PersonalFinanceManager.API   (controllers, JWT auth, Swagger)
        │
        ▼
PersonalFinanceManager.Application   ◀── NEW  (use-case services, DTOs, validation, mapping, ICurrentUser)
        │
        ▼
PersonalFinanceManager.Infrastructure   (EF Core, repositories, UnitOfWork, JWT/password services)
        │
        ▼
PersonalFinanceManager.Core   (domain entities, value objects, interfaces)
```

Dependency rule: every arrow points inward toward Core. The API depends on Application; Application depends on Core (and defines abstractions Infrastructure implements).

---

## 4. Cross-cutting design rules (apply in every phase)

1. **One use-case = one transaction.** Application services orchestrate domain calls and commit once via `IUnitOfWork.SaveChangesAsync()` (or `BeginTransaction…Commit` for multi-step). Example — *create expense*: load account → `account.Debit(amount)` → load matching budget → `budget.RecordSpending(amount, allowExceed: true)` → add transaction → save. *Delete/edit* must reverse the account balance **and** `budget.ReverseSpending(...)`.
2. **Budget spend has a single owner.** `Budget.CurrentSpend` is the source of truth and is only ever changed by the transaction use-cases (never recomputed ad-hoc in two places). `TransactionRepository.GetTotalByType…` is for reporting only.
3. **Ownership is enforced server-side.** Every query is scoped by the authenticated `UserId` taken from JWT claims via `ICurrentUser` — never from the request body. A user can never read/modify another user's data.
4. **DTOs in, DTOs out.** Controllers accept request DTOs and return response DTOs; mapping lives in Application. Domain entities never cross the API boundary.
5. **Throw domain exceptions; let middleware translate.** Handlers/services throw `EntityNotFoundException`, `InsufficientFundsException`, etc.; `GlobalExceptionHandlerMiddleware` maps them to status codes. Add `UnauthorizedException` → 401/403 mapping in Phase 1.
6. **Money:** store `decimal` + `CurrencyCode` (existing pattern). Reject cross-currency operations (the domain already does). Multi-currency conversion is out of scope for the MVP.

---

## 5. Phases

### Phase 0 — Stabilize the foundation
**Goal:** the solution builds and runs without destroying data.

- [x] API `Program.cs`: replace `DatabaseInitializer.ResetDatabaseAsync` with migration-based startup (added `DatabaseInitializer.MigrateDatabaseAsync` → `context.Database.MigrateAsync()`). `ResetDatabaseAsync` retained for deliberate dev resets.
- [x] Add EF Core tooling and create the initial migration:
  - `dotnet tool install --global dotnet-ef` (once)
  - `dotnet ef migrations add InitialCreate -p PersonalFinanceManager.Infrastructure -s PersonalFinanceManager.API`
  - `dotnet ef database update -p PersonalFinanceManager.Infrastructure -s PersonalFinanceManager.API`
- [x] Bump `PersonalFinanceManager.Web` to `net8.0` (align with the rest); restores cleanly (remaining Web errors are code-level, deferred to Phase 3).
- [x] Get the API to run cleanly (`/health`, `/api/health`, `/api/health/db` all Healthy; startup applies migrations, no reset).
- [x] Confirm `appsettings` connection strings are correct for LocalDB (`PersonalFinanceManagerDb_Dev` in Development).

**Acceptance:** ✅ API builds, runs, applies migrations without dropping the DB; `/health/db` returns Healthy.
**Completion note (2026-06-24):** Also fixed an EF design-time crash — `Microsoft.EntityFrameworkCore.Design` was 7.0.18 against an 8.0.10 runtime; pinned to 8.0.7 (feed max) and added a direct `System.Collections.Immutable` 8.0.0 reference to clear the resulting NU1605 downgrade.
**Resources:** EF Core Migrations — https://learn.microsoft.com/ef/core/managing-schemas/migrations/

---

### Phase 1 — Application layer + custom JWT auth
**Goal:** an authenticated user can register and log in; the API knows "who am I."

- [x] Create `PersonalFinanceManager.Application` classlib (net8.0); references Core only. Added to solution. API references Application; Application has no reference back to API.
- [x] **Auth abstractions:** `IPasswordHasher`, `IJwtTokenGenerator` (in Core/Interfaces, implemented by Infrastructure); `ICurrentUser` (in Application, implemented by API).
- [x] **Infrastructure implementations:**
  - `Pbkdf2PasswordHasher` (PBKDF2 / `Rfc2898DeriveBytes`, HMAC-SHA256, salted, constant-time verify) — **BCrypt.Net-Next is not in the feed**, so PBKDF2 was used (no external dependency).
  - `JwtTokenGenerator` (HS256; claims `sub`/email/given_name/family_name/name/jti/exp). Settings bound from the `Jwt` config section (Issuer, Audience, Key, ExpiryMinutes).
- [x] **API `CurrentUser`** reads `HttpContext.User` claims (`sub`/`email`, with `ClaimTypes` fallbacks); `IHttpContextAccessor` registered.
- [x] **Application auth service:** `AuthAppService.RegisterAsync` / `LoginAsync` returning `AuthResult` (shape matches Web `AuthResponseModel`).
- [x] **API pipeline:** `AddAuthentication().AddJwtBearer(...)` with validation params matching the generator; `UseAuthentication()` before `UseAuthorization()`.
- [x] **`AuthController`:** `POST api/auth/register`, `POST api/auth/login`, `[Authorize] GET api/auth/me`.
- [x] Add `UnauthorizedException` (Core) and map it to 401 in `GlobalExceptionHandlerMiddleware` (arm placed above the `DomainException` catch-all).
- [x] Seed 12 default categories (8 expense + 4 income) for a new user on registration, committed atomically via the `User` aggregate.

**Acceptance:** ✅ register + login return a valid JWT; `[Authorize] /api/auth/me` rejects no/invalid token (401) and accepts a valid one; claims resolve through `ICurrentUser`. Negative paths verified: bad password 401 (generic message), duplicate email 409, invalid model 400. DB check: 2 users → 24 categories (12 each).
**Completion note (2026-06-24):** JWT stack pinned to the 6.x IdentityModel major to fit the feed — `Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.8 (feed max) + `System.IdentityModel.Tokens.Jwt` 6.35.0; added `Microsoft.Extensions.Options.ConfigurationExtensions` 8.0.0 for settings binding. No schema change → no new migration.
**Resources:** JWT auth in ASP.NET Core — https://learn.microsoft.com/aspnet/core/security/authentication/configure-jwt-bearer-authentication

---

### Phase 2 — API surface (domain controllers)
**Goal:** full CRUD + use-cases for every resource, owned by the current user, documented in Swagger. All controllers `[Authorize]` and inherit `BaseApiController`.

- [x] **Request/response DTOs** in Application for each resource (align response DTOs with the Web `*Dto` shapes: `AccountDto`, `TransactionDto`, `CategoryDto`, `BudgetDto`, `DashboardDto`). Each DTO has a static `FromEntity()` mapper.
- [x] **Application services** (one per aggregate area), each scoped by `ICurrentUser.UserId`:
  - `AccountAppService` — list/get/create/update/activate/deactivate/delete.
  - `CategoryAppService` — list (filter by type)/get/create/delete.
  - `TransactionAppService` — list (paged)/by-account/recent/get/**create income+expense+transfer** (Design Rule #1: account balance + budget spend in one `SaveChangesAsync`)/delete (reverses balance and budget spend).
  - `BudgetAppService` — list/active/get/create/update/delete.
  - `DashboardAppService` — total balance, monthly income/expense, recent transactions, active budgets, spending-by-category breakdown.
- [x] **Controllers:** `AccountsController`, `CategoriesController`, `TransactionsController`, `BudgetsController`, `DashboardController` — all `[Authorize]`, thin pass-throughs; 201+`CreatedAtAction` on create, 204 on delete, domain exceptions mapped by middleware.
- [x] **Swagger:** JWT bearer security definition (Authorize button) added in Phase 1; already in place.
- [x] Register all Application services in `AddApplication()` DI extension.

**Acceptance:** ✅ All endpoints verified end-to-end via curl: create account (balance=1000) → add budget (limit=500) → POST expense $150 (balance→850, currentSpend→150, remaining→350, pct=30%) → DELETE transaction (balance→1000, currentSpend→0, remaining→500). Dashboard returns totalBalance, income/expense totals. Cross-phase ownership enforcement (404 on wrong-user access) in place via `RequireOwnedXxx` guards.
**Completion note (2026-07-03):** All services and controllers implemented. `TransactionAppService` is the key orchestrator enforcing Design Rule #1. `PagedResult<T>` added for paginated transaction lists.

---

### Phase 3 — Web end-to-end (Blazor Server)
**Goal:** first fully working product — a user can do everything through the browser.

- [ ] **Fix the build first:**
  - Reconcile `ApiResult` vs `ApiResponse` (pick one result wrapper; update `IAuthService`/`AuthService` and all feature services consistently).
  - Reconcile `IApiClient` method set with actual calls (e.g. the `PostAsync<TResponse>(url, data)` overload `AuthService` expects). Recreate the deleted request models (`CreateAccountModel`, `CreateBudgetModel`, `CreateTransactionModel`) and un-exclude/restore `CreateCategoryModel`.
- [ ] **Auth UX:** `Login.razor` / `Register.razor` wired to `AuthService`; `CascadingAuthenticationState` + `AuthorizeRouteView`; `RedirectToLogin`; logout clears token. Confirm `AuthTokenHandler` attaches the token to every API call.
- [ ] **Feature pages** against the live API: Accounts (list/detail/create/activate), Categories (list/create), Transactions (paged list/create incl. transfer), Budgets (list/create with progress), Dashboard.
- [ ] **Dashboard analytics:** charts via Chart.js (JS interop) — spend by category, income vs expense, budget usage.
- [ ] Clean out template scaffolding (`WeatherForecast*`, `Counter.razor`, `FetchData.razor`, duplicate layouts under `Shared/` vs `Components/Layout/`).

**Acceptance:** register → log in → create account → add categories → record income/expense/transfer → see balances and budget progress update → dashboard charts render. End-to-end, no console errors.
**Completion note (2026-07-03):** Web project builds cleanly (0 errors). Key changes:
- Added `IDashboardService`/`DashboardService` calling `GET /api/dashboard`; Dashboard.razor now uses the single API endpoint instead of three parallel calls. Monthly income/expense comes from the server-computed `DashboardDto`.
- Added `PagedResultDto<T>` model; `ITransactionService.GetPagedAsync` replaces `GetAllAsync`; `TransactionList.razor` uses server-side pagination via `Pagination.razor` component and confirms deletes with `ConfirmDialog`.
- `BudgetList.razor` and `CategoryList.razor` now confirm destructive deletes with `ConfirmDialog` (previously immediate on click).
- Chart.js 4.4.2 added to `_Layout.cshtml`; `app.js` extended with `chartHelper.renderDoughnut` (spending by category) and `renderIncomeExpenseBar` (income vs expenses); Dashboard.razor renders both via `IJSRuntime.InvokeVoidAsync`.
- Created `Pages/Reports.razor` (fixes broken `/reports` nav link) — full summary: KPI cards, both charts, recent transaction table, account balances.
- Fixed `MainLayout.razor` `ToastContainer` fully-qualified name → bare `<ToastContainer />` (import already in `_Imports.razor`).
- Deleted dead `Models/ApiResponse.cs`.

---

### Phase 4 — Quality (tests + CI)
**Goal:** confidence to refactor and extend.

- [ ] `PersonalFinanceManager.UnitTests` (xUnit): domain invariants (`Account.Debit` overdraft rules, `Budget.RecordSpending`/`Reverse`, `User` factory uniqueness) and Application services (mock `IUnitOfWork`/repos with Moq).
- [ ] `PersonalFinanceManager.IntegrationTests`: `WebApplicationFactory` + EF InMemory or SQLite; auth flow + a couple of transactional use-cases (expense debits account + budget; delete reverses).
- [ ] GitHub Actions workflow: restore → build → test on push/PR.

**Acceptance:** `dotnet test` green locally and in CI; transactional consistency covered by tests.
**Completion note (2026-07-03):** 132 tests all green (122 unit + 10 integration).
- `PersonalFinanceManager.UnitTests`: 101 domain tests (Account, Budget, User, Transaction, BaseEntity, Money, DateRange) + 21 Application service tests (Auth, Account, Budget, TransactionAppService) using Moq for all repository/UoW dependencies.
- `PersonalFinanceManager.IntegrationTests`: EF SQLite in-memory via `CustomWebApplicationFactory` (abstract base with per-class concrete subclasses for isolated databases); JWT settings injected via `builder.UseSetting()`; tests cover auth flow (4), transactional consistency (4), and cross-user ownership enforcement (2).
- `Program.cs` modified: `public partial class Program { }` added for `WebApplicationFactory<Program>`; `MigrateDatabaseAsync` guarded with `if (!app.Environment.IsEnvironment("Testing"))`.
- `.github/workflows/ci.yml` created: push/PR to main triggers restore → build (Release) → unit test → integration test.
- Feed-constrained package versions used: `Microsoft.NET.Test.Sdk` 17.10.0, `Moq` 4.20.70, `xunit` 2.9.2, `FluentAssertions` 6.12.0, `Microsoft.EntityFrameworkCore.Sqlite` 8.0.5, `Microsoft.AspNetCore.Mvc.Testing` 8.0.7.
**Resources:** Unit testing .NET — https://learn.microsoft.com/dotnet/core/testing/ · Integration tests — https://learn.microsoft.com/aspnet/core/test/integration-tests

---

### Phase 5 — Desktop client (WPF) — *committed*
**Goal:** a Windows desktop app for offline entry that syncs to the API.

- [ ] `PersonalFinanceManager.Desktop` (WPF, net8.0-windows), MVVM, `Microsoft.Extensions.DependencyInjection` host. Reuse Core models and DTOs (extract shared DTOs into a `Shared`/`Application.Contracts` library so Web, Desktop, and API consume one definition).
- [ ] **API client** (`HttpClient` + JWT) mirroring the Web service layer; login screen storing the token securely (DPAPI).
- [ ] **Offline store:** local **SQLite** (EF Core SQLite provider) holding transactions entered offline.
- [ ] **Sync service:** push queued local changes to the API and pull updates; define a conflict-resolution policy (last-write-wins by `UpdatedAt`, or per-entity rules). This is the headline learning piece of this phase.
- [ ] Views: dashboard, transaction entry, reports; data import/export (CSV/Excel).

**Acceptance:** create transactions offline → reconnect → sync reconciles with the API without data loss or duplication.
**Resources:** WPF — https://learn.microsoft.com/dotnet/desktop/wpf/ · EF Core SQLite — https://learn.microsoft.com/ef/core/providers/sqlite/

**Design-for-now (so the full suite drops in later):** extract shared DTOs/contracts into one library in this phase; keep the API the single source of truth; keep auth stateless (JWT) so Mobile reuses it unchanged.

---

### Phase 6+ — Full suite — *planned (after Phase 5 dev tests pass)*

- **Mobile (.NET MAUI):** reuse the shared contracts + API client; quick expense entry, offline cache, push notifications for budget alerts, biometric unlock.
- **Cloud + DevOps:** Azure SQL, App Service for API + Web, CI/CD pipeline, monitoring/logging (App Insights).
- **Advanced:** SignalR (real-time budget/transaction updates), ML.NET (spend prediction / anomaly detection), Hangfire (recurring-transaction generation, budget-period rollover, alert jobs).

**Resources:** .NET MAUI — https://learn.microsoft.com/dotnet/maui/ · Azure App Service — https://learn.microsoft.com/azure/app-service/ · SignalR — https://learn.microsoft.com/aspnet/core/signalr/ · ML.NET — https://learn.microsoft.com/dotnet/machine-learning/ · Hangfire — https://www.hangfire.io/

---

## 6. Top risks & mitigations

| Risk | Mitigation |
|---|---|
| Budget spend drifts from transactions | Single-owner rule (#2); cover with integration tests in Phase 4. |
| Auth/ownership gaps leak cross-user data | Always scope by `ICurrentUser.UserId` server-side (#3); negative tests. |
| `EnsureCreated`/reset destroys data | Switch to migrations in Phase 0 before building features. |
| DTO duplication across Web/Desktop/Mobile | Extract a shared contracts library in Phase 5. |
| Scope creep into the full suite too early | Gate Phase 6+ on Phase 5 passing dev tests (your stated plan). |
