using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinanceManager.Core.Entities;

namespace PersonalFinanceManager.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");

		builder.HasKey(u => u.Id);

		builder.Property(u => u.Email)
			.IsRequired()
			.HasMaxLength(256);

		builder.HasIndex(u => u.Email)
			.IsUnique();

		builder.Property(u => u.FirstName)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(u => u.LastName)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(u => u.PasswordHash)
			.IsRequired()
			.HasMaxLength(512);

		builder.Property(u => u.DefaultCurrency)
			.HasConversion<string>()
			.HasMaxLength(3);

		
		// Map the Address value object as an owned entity (stored in the same table)
		builder.OwnsOne(u => u.Address, address =>
		{
			address.Property(a => a.Street).HasMaxLength(200).HasColumnName("Address_Street");
			address.Property(a => a.City).HasMaxLength(100).HasColumnName("Address_City");
			address.Property(a => a.State).HasMaxLength(100).HasColumnName("Address_State");
			address.Property(a => a.PostalCode).HasMaxLength(20).HasColumnName("Address_PostalCode");
			address.Property(a => a.Country).HasMaxLength(100).HasColumnName("Address_Country");
		});

		
		// Relationships
		builder.HasMany(u => u.Accounts)
			.WithOne(a => a.User)
			.HasForeignKey(a => a.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(u => u.Categories)
			.WithOne(c => c.User)
			.HasForeignKey(c => c.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasMany(u => u.Budgets)
			.WithOne(b => b.User)
			.HasForeignKey(b => b.UserId)
			.OnDelete(DeleteBehavior.Cascade);


		// Configure backing field access for read-only collections
		builder.Navigation(u => u.Accounts)
			.UsePropertyAccessMode(PropertyAccessMode.Field);

		builder.Navigation(u => u.Categories)
			.UsePropertyAccessMode(PropertyAccessMode.Field);

		builder.Navigation(u => u.Budgets)
			.UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}

