using PersonalFinanceManager.IntegrationTests.Infrastructure;

namespace PersonalFinanceManager.IntegrationTests.Auth;

/// <summary>
/// Integration tests for the auth endpoints.
/// Each test uses a unique email to avoid duplicate-email conflicts
/// within the shared per-class SQLite database.
/// </summary>
public class AuthEndpointsTests : IClassFixture<AuthTestFactory>
{
    private readonly AuthTestFactory _factory;

    public AuthEndpointsTests(AuthTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient NewClient() => _factory.CreateClient();

    // ── Register ──────────────────────────────────────────

    [Fact]
    public async Task Register_ValidRequest_Returns200WithToken()
    {
        var client = NewClient();
        var email  = $"{Guid.NewGuid():N}@test.com";

        var response = await client.PostAsJsonAsync("api/auth/register", new
        {
            firstName = "Alice", lastName = "Smith",
            email, password = "Password1!", defaultCurrency = "USD"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        body.GetProperty("email").GetString().Should().Be(email);
    }

    // ── GetMe ─────────────────────────────────────────────

    [Fact]
    public async Task GetMe_ValidToken_Returns200WithUserId()
    {
        var client = NewClient();
        var (token, _) = await ApiHelpers.RegisterUserAsync(client);
        ApiHelpers.SetBearerToken(client, token);

        var response = await client.GetAsync("api/auth/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        body.GetProperty("isAuthenticated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetMe_NoToken_Returns401()
    {
        var client = NewClient();
        // No Authorization header set

        var response = await client.GetAsync("api/auth/me");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    // ── Login ─────────────────────────────────────────────

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = NewClient();
        var email  = $"{Guid.NewGuid():N}@test.com";

        // Register first
        await client.PostAsJsonAsync("api/auth/register", new
        {
            firstName = "Bob", lastName = "Jones",
            email, password = "Correct1!", defaultCurrency = "USD"
        });

        // Login with wrong password
        var response = await client.PostAsJsonAsync("api/auth/login", new
        {
            email, password = "WrongPassword!"
        });

        // GlobalExceptionHandlerMiddleware maps UnauthorizedException → 401
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
