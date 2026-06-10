using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
	public void Configure(EntityTypeBuilder<Budget> builder)
	{
		builder.ToTable("Budgets");

		builder.HasKey(b => b.Id);

		builder.Property(b => b.Name)
			.IsRequired()
			.HasMaxLength(150);

		builder.Property(b => b.Limit)
			.HasPrecision(18, 2);

		builder.Property(b => b.CurrentSpend)
			.HasPrecision(18, 2);

		builder.Property(b => b.Currency)
			.HasConversion<string>()
			.HasMaxLength(3);

		builder.Property(b => b.Period)
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.HasIndex(b => b.UserId);
		builder.HasIndex(b => b.CategoryId);

		builder.HasOne(b => b.Category)
			.WithMany(c => c.Budgets)
			.HasForeignKey(b => b.CategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		// Computed read-only properties, no backing column/not stored in the DB
		builder.Ignore(b => b.Remaining);
		builder.Ignore(b => b.PercentageUsed);
		builder.Ignore(b => b.IsExceeded);
		builder.Ignore(b => b.IsActive);
	}
}