using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;

namespace Ultramaverick.Identity.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", "Identity");
            builder.HasKey(u => u.Id);
            builder.Ignore(u => u.DomainEvents);

            builder.Property(u => u.UserName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            builder.HasIndex(u => u.UserName).IsUnique();

            builder.Property(u => u.Password)
                .HasConversion(p => p.Value, value => PasswordHash.FromEncoded(value))
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired();


            builder.Property(u => u.RoleId).IsRequired();
            builder.Property(u => u.DepartmentId).IsRequired();
            builder.Property(u => u.IsActive).IsRequired();
            builder.Property(u => u.CreatedAtUtc).IsRequired();
            builder.Property<byte[]>("RowVersion").IsRowVersion();

            builder.HasOne<Role>().WithMany().HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Department>().WithMany().HasForeignKey(u => u.DepartmentId).OnDelete(DeleteBehavior.Restrict);

        }
    }
}
