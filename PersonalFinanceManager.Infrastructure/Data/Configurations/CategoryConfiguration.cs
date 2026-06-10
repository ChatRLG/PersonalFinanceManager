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

		// Composite unique index: no duplicate name+type per user (only for non-deleted)
		builder.HasIndex(c => new { c.UserId, c.Name, c.Type })
			.IsUnique()
			.HasFilter("[IsDeleted] = 0");
	}
}