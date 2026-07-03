using PersonalFinanceManager.Application.Auth.Dtos;
using PersonalFinanceManager.Core.Entities;
using PersonalFinanceManager.Core.Enums;
using PersonalFinanceManager.Core.Exceptions;
using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Auth;

/// <summary>
/// Handles registration and login. Passwords are hashed via <see cref="IPasswordHasher"/>;
/// JWTs are produced via <see cref="IJwtTokenGenerator"/>. All persistence goes through
/// the unit of work so a registration (user + seeded categories) commits atomically.
/// </summary>
public class AuthAppService : IAuthAppService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IPasswordHasher _passwordHasher;
	private readonly IJwtTokenGenerator _tokenGenerator;

	public AuthAppService(
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher,
		IJwtTokenGenerator tokenGenerator)
	{
		_unitOfWork = unitOfWork;
		_passwordHasher = passwordHasher;
		_tokenGenerator = tokenGenerator;
	}

	public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
	{
		var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

		if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
			throw new InvalidOperationException($"An account with email '{email}' already exists.");

		var passwordHash = _passwordHasher.Hash(request.Password);

		// User constructor validates email/name and normalises the email.
		var user = new User(email, request.FirstName, request.LastName, passwordHash, request.DefaultCurrency);

		SeedDefaultCategories(user);

		await _unitOfWork.Users.AddAsync(user, cancellationToken);
		await _unitOfWork.SaveChangesAsync(cancellationToken);

		return BuildAuthResult(user);
	}

	public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
	{
		var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

		var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

		// Same error whether the user is missing or the password is wrong — avoid leaking which.
		if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
			throw new UnauthorizedException("Invalid email or password.");

		return BuildAuthResult(user);
	}

	private AuthResult BuildAuthResult(User user)
	{
		var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);

		return new AuthResult
		{
			Token = token,
			Email = user.Email,
			FirstName = user.FirstName,
			LastName = user.LastName,
			Expiration = expiresAtUtc
		};
	}

	/// <summary>
	/// Gives a new user a starter set of income/expense categories so the app
	/// is usable immediately. Added through the aggregate so invariants hold;
	/// EF cascades the inserts when the user is saved.
	/// </summary>
	private static void SeedDefaultCategories(User user)
	{
		string[] expenseCategories =
		{
			"Groceries", "Rent", "Utilities", "Transport",
			"Dining", "Entertainment", "Healthcare", "Shopping"
		};

		string[] incomeCategories = { "Salary", "Bonus", "Interest", "Gifts" };

		foreach (var name in expenseCategories)
			user.AddCategory(name, TransactionType.Expense);

		foreach (var name in incomeCategories)
			user.AddCategory(name, TransactionType.Income);
	}
}
