using PersonalFinanceManager.Application.Accounts;
using PersonalFinanceManager.Application.Accounts.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.UnitTests.Application;

public class AccountAppServiceTests
{
    private readonly User _user;
    private readonly Guid _userId;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IAccountRepository> _mockAccounts;
    private readonly Mock<IUserRepository>    _mockUsers;
    private readonly Mock<ICurrentUser>       _mockCurrentUser;
    private readonly AccountAppService _sut;

    public AccountAppServiceTests()
    {
        _user   = new User("t@t.com", "T", "T", "hash", CurrencyCode.USD);
        _userId = _user.Id;

        _mockAccounts    = new Mock<IAccountRepository>();
        _mockUsers       = new Mock<IUserRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Accounts).Returns(_mockAccounts.Object);
        _uow.Setup(u => u.Users).Returns(_mockUsers.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockCurrentUser.Setup(c => c.UserId).Returns(_userId);

        _sut = new AccountAppService(_uow.Object, _mockCurrentUser.Object);
    }

    private Account MakeOwnedAccount(decimal balance = 1000m, AccountType type = AccountType.Checking)
        => _user.AddAccount($"Acc-{Guid.NewGuid():N}", type, CurrencyCode.USD, balance);

    // ── CreateAsync ───────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_LoadsUser_AddsAccount_Saves_ReturnsDto()
    {
        _mockUsers.Setup(r => r.GetWithAccountsAsync(_userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(_user);
        _mockAccounts.Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Account a, CancellationToken _) => a);

        var request = new CreateAccountRequest
        {
            Name = "Main", Type = AccountType.Checking,
            Currency = CurrencyCode.USD, InitialBalance = 500m
        };

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Main");
        result.Balance.Should().Be(500m);
        _mockAccounts.Verify(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateAccountName_PropagatesInvalidOperationException()
    {
        // _user already has "Main"; adding "Main" again triggers domain invariant
        _user.AddAccount("Main", AccountType.Checking, CurrencyCode.USD, 0m);
        _mockUsers.Setup(r => r.GetWithAccountsAsync(_userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(_user);

        var act = () => _sut.CreateAsync(new CreateAccountRequest
        {
            Name = "Main", Type = AccountType.Savings,
            Currency = CurrencyCode.USD, InitialBalance = 0m
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── DeleteAsync ───────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_OwnedAccount_CallsDeleteAsync_Saves()
    {
        var account = MakeOwnedAccount();
        _mockAccounts.Setup(r => r.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(account);
        _mockAccounts.Setup(r => r.DeleteAsync(account.Id, It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(account.Id);

        _mockAccounts.Verify(r => r.DeleteAsync(account.Id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WrongUserAccount_ThrowsEntityNotFoundException()
    {
        // Create an account owned by a DIFFERENT user
        var otherUser    = new User("other@x.com", "O", "U", "hash", CurrencyCode.USD);
        var otherAccount = otherUser.AddAccount("OtherAcc", AccountType.Checking, CurrencyCode.USD, 0m);

        _mockAccounts.Setup(r => r.GetByIdAsync(otherAccount.Id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(otherAccount);

        var act = () => _sut.DeleteAsync(otherAccount.Id);

        // Ownership masked as EntityNotFoundException
        await act.Should().ThrowAsync<EntityNotFoundException>();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
