using System.Net;
using PersonalFinanceManager.IntegrationTests.Infrastructure;

namespace PersonalFinanceManager.IntegrationTests.Accounts;

/// <summary>
/// Verifies that users cannot read or modify accounts they don't own.
/// Ownership failures are masked as 404 (EntityNotFoundException) so that
/// attackers cannot enumerate other users' resource IDs.
/// </summary>
public class CrossUserOwnershipTests : IClassFixture<OwnershipTestFactory>
{
    private readonly OwnershipTestFactory _factory;

    public CrossUserOwnershipTests(OwnershipTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient NewClient() => _factory.CreateClient();

    [Fact]
    public async Task UserB_GetAccountOwnedByUserA_Returns404()
    {
        var clientA = NewClient();
        var (tokenA, _) = await ApiHelpers.RegisterUserAsync(clientA);
        ApiHelpers.SetBearerToken(clientA, tokenA);
        var account = await ApiHelpers.CreateAccountAsync(clientA, "UserA Account");
        var accountId = account.GetProperty("id").GetString()!;

        // User B tries to GET User A's account
        var clientB = NewClient();
        var (tokenB, _) = await ApiHelpers.RegisterUserAsync(clientB);
        ApiHelpers.SetBearerToken(clientB, tokenB);

        var response = await clientB.GetAsync($"api/accounts/{accountId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "ownership failures are masked as 404 to prevent resource enumeration");
    }

    [Fact]
    public async Task UserB_DeleteAccountOwnedByUserA_Returns404()
    {
        var clientA = NewClient();
        var (tokenA, _) = await ApiHelpers.RegisterUserAsync(clientA);
        ApiHelpers.SetBearerToken(clientA, tokenA);
        var account = await ApiHelpers.CreateAccountAsync(clientA, "UserA Account 2");
        var accountId = account.GetProperty("id").GetString()!;

        // User B tries to DELETE User A's account
        var clientB = NewClient();
        var (tokenB, _) = await ApiHelpers.RegisterUserAsync(clientB);
        ApiHelpers.SetBearerToken(clientB, tokenB);

        var response = await clientB.DeleteAsync($"api/accounts/{accountId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "ownership failures are masked as 404");
    }
}
