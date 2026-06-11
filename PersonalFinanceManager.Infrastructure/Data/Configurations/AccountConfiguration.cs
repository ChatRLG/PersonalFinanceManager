using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
	public void Configure(EntityTypeBuilder<Account> builder)
	{
		builder.ToTable("Accounts");

		builder.HasKey(a => a.Id);

		builder.Property(a => a.Name)
			.IsRequired()
			.HasMaxLength(150);

		builder.Property(a => a.Description)
			.HasMaxLength(500);

		builder.Property(a => a.Type)
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.Property(a => a.Currency)
			.HasConversion<string>()
			.HasMaxLength(3);

		builder.Property(a => a.Balance)
			.HasPrecision(18, 2);

		builder.Property(a => a.IsActive)
			.IsRequired();

		builder.HasIndex(a => a.UserId);

		// Explicitly tell EF to use the backing field for the Transactions collection
		builder.HasMany(a => a.Transactions)
			.WithOne(t => t.Account)
			.HasForeignKey(t => t.AccountId)
			.OnDelete(DeleteBehavior.Restrict);

		//// Ignore computed property — not stored in DB
		//builder.Ignore(a => a.IsActive);

		builder.Navigation(a => a.Transactions)
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}