using PersonalFinanceManager.Application.Budgets;
using PersonalFinanceManager.Application.Budgets.Dtos;
using PersonalFinanceManager.Application.Common.Interfaces;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.UnitTests.Application;

public class BudgetAppServiceTests
{
    private readonly User _user;
    private readonly Guid _userId;
    private readonly Mock<IUnitOfWork>      _uow;
    private readonly Mock<IBudgetRepository> _mockBudgets;
    private readonly Mock<IUserRepository>   _mockUsers;
    private readonly Mock<ICurrentUser>      _mockCurrentUser;
    private readonly BudgetAppService _sut;

    private static readonly DateTime _start =
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _end =
        new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc);

    public BudgetAppServiceTests()
    {
        _user   = new User("t@t.com", "T", "T", "hash", CurrencyCode.USD);
        _userId = _user.Id;

        _mockBudgets     = new Mock<IBudgetRepository>();
        _mockUsers       = new Mock<IUserRepository>();
        _mockCurrentUser = new Mock<ICurrentUser>();

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Budgets).Returns(_mockBudgets.Object);
        _uow.Setup(u => u.Users).Returns(_mockUsers.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockCurrentUser.Setup(c => c.UserId).Returns(_userId);

        _sut = new BudgetAppService(_uow.Object, _mockCurrentUser.Object);
    }

    // ── CreateAsync ───────────────────────────────────────

    [Fact]
    public async Task CreateAsync_EndDateBeforeStartDate_ThrowsBeforeAnyRepoCall()
    {
        var request = new CreateBudgetRequest
        {
            Name = "Bad", Limit = 500m, Currency = CurrencyCode.USD,
            Period = BudgetPeriod.Monthly,
            StartDate = _end, EndDate = _start, // reversed
            CategoryId = Guid.NewGuid()
        };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
        // The guard fires before any repo call
        _mockUsers.Verify(r => r.GetWithFullProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CallsAddBudget_Saves_ReturnsDto()
    {
        // Add a category to the user so AddBudget doesn't throw
        var cat = _user.AddCategory("Groceries", TransactionType.Expense);

        _mockUsers.Setup(r => r.GetWithFullProfileAsync(_userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(_user);
        _mockBudgets.Setup(r => r.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Budget b, CancellationToken _) => b);
        // Reload after save returns null → service falls back to in-memory budget object
        _mockBudgets.Setup(r => r.GetWithCategoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Budget?)null);

        var request = new CreateBudgetRequest
        {
            Name = "Groceries", Limit = 500m, Currency = CurrencyCode.USD,
            Period = BudgetPeriod.Monthly, StartDate = _start, EndDate = _end,
            CategoryId = cat.Id
        };

        var result = await _sut.CreateAsync(request);

        result.Name.Should().Be("Groceries");
        result.Limit.Should().Be(500m);
        _mockBudgets.Verify(r => r.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_UserNotFound_ThrowsEntityNotFoundException()
    {
        _mockUsers.Setup(r => r.GetWithFullProfileAsync(_userId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((User?)null);

        var act = () => _sut.CreateAsync(new CreateBudgetRequest
        {
            Name = "X", Limit = 100m, Currency = CurrencyCode.USD,
            Period = BudgetPeriod.Monthly, StartDate = _start, EndDate = _end,
            CategoryId = Guid.NewGuid()
        });

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
