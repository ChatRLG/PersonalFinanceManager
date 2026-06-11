using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
	public void Configure(EntityTypeBuilder<Category> builder)
	{
		builder.ToTable("Categories");

		builder.HasKey(c => c.Id);

		builder.Property(c => c.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(c => c.Type)
			.HasConversion<string>()
			.HasMaxLength(50);

		builder.Property(c => c.Icon)
			.HasMaxLength(50);

		builder.Property(c => c.Colour)
			.HasMaxLength(7);

		builder.HasIndex(c => c.UserId);

		// Composite unique index: same user can't have duplicate names per type
		// Filtered to only non-deleted rows
		builder.HasIndex(c => new { c.UserId, c.Name, c.Type })
			.IsUnique()
			.HasFilter("[IsDeleted] = 0");

		// Tell EF how to access the private backing field for the Budgets collection
		builder.HasMany(c => c.Budgets)
			.WithOne(b => b.Category)
			.HasForeignKey(b => b.CategoryId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Navigation(c => c.Budgets)
			.UsePropertyAccessMode(PropertyAccessMode.Field);

		// Tell EF how to access the private backing field for the Transactions collection
		builder.Navigation(c => c.Transactions)
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}