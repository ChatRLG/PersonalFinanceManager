using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Core.Entities.Transaction>
{
	public void Configure(EntityTypeBuilder<Transaction> builder)
	{
		builder.ToTable("Transactions");

		builder.HasKey(t => t.Id);

		builder.Property(t => t.Amount)
			.HasPrecision(18, 2);

		builder.Property(t => t.Currency)
			.HasConversion<string>()
			.HasMaxLength(3);

		builder.Property(t => t.Type)
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.Property(t => t.Description)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(t => t.Notes)
			.HasMaxLength(1000);

		builder.Property(t => t.TransactionDate)
			.IsRequired();

		builder.Property(t => t.IsRecurring)
			.IsRequired();

		// Source account relationship
		builder.HasOne(t => t.Account)
			.WithMany(a => a.Transactions)
			.HasForeignKey(t => t.AccountId)
			.OnDelete(DeleteBehavior.Restrict);

		// Destination account (transfers only)
		builder.HasOne(t => t.DestinationAccount)
			.WithMany()
			.HasForeignKey(t => t.DestinationAccountId)
			.OnDelete(DeleteBehavior.Restrict)
			.IsRequired(false);

		// Category relationship
		builder.HasOne(t => t.Category)
			.WithMany(c => c.Transactions)
			.HasForeignKey(t => t.CategoryId)
			.OnDelete(DeleteBehavior.Restrict);


		builder.HasIndex(t => t.AccountId);
		builder.HasIndex(t => t.Type);
		builder.HasIndex(t => t.TransactionDate);

	}
}