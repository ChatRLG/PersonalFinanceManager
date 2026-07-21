using PersonalFinanceManager.Application.Auth;
using PersonalFinanceManager.Application.Auth.Dtos;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.UnitTests.Application;

public class AuthAppServiceTests
{
    private readonly Mock<IUnitOfWork> _uow;
    private readonly Mock<IUserRepository> _mockUsers;
    private readonly Mock<IPasswordHasher> _hasher;
    private readonly Mock<IJwtTokenGenerator> _tokenGen;
    private readonly AuthAppService _sut;

    public AuthAppServiceTests()
    {
        _mockUsers = new Mock<IUserRepository>();
        _hasher    = new Mock<IPasswordHasher>();
        _tokenGen  = new Mock<IJwtTokenGenerator>();

        _uow = new Mock<IUnitOfWork>();
        _uow.Setup(u => u.Users).Returns(_mockUsers.Object);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _sut = new AuthAppService(_uow.Object, _hasher.Object, _tokenGen.Object);
    }

    // ── RegisterAsync ─────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        _mockUsers.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        var act = () => _sut.RegisterAsync(new RegisterRequest
        {
            Email = "a@b.com", FirstName = "A", LastName = "B",
            Password = "pass", DefaultCurrency = CurrencyCode.USD
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_Seeds12Categories_SavesOnce_ReturnsToken()
    {
        _mockUsers.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokenGen.Setup(t => t.GenerateToken(It.IsAny<User>()))
                 .Returns(("tok", DateTime.UtcNow.AddHours(1)));

        User? captured = null;
        _mockUsers.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                  .Callback<User, CancellationToken>((u, _) => captured = u)
                  .ReturnsAsync((User u, CancellationToken _) => u);

        var result = await _sut.RegisterAsync(new RegisterRequest
        {
            Email = "new@x.com", FirstName = "New", LastName = "User",
            Password = "pass", DefaultCurrency = CurrencyCode.USD
        });

        result.Token.Should().Be("tok");
        captured.Should().NotBeNull();
        captured!.Categories.Should().HaveCount(12, "8 expense + 4 income categories are seeded");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── LoginAsync ────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedException()
    {
        _mockUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((User?)null);

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "x@y.com", Password = "pw" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        var user = new User("a@b.com", "A", "B", "hashed", CurrencyCode.USD);
        _mockUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = () => _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "wrong" });

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User("a@b.com", "A", "B", "hashed", CurrencyCode.USD);
        _mockUsers.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _tokenGen.Setup(t => t.GenerateToken(It.IsAny<User>()))
                 .Returns(("tok123", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "correct" });

        result.Token.Should().Be("tok123");
        result.Email.Should().Be("a@b.com");
    }
}
