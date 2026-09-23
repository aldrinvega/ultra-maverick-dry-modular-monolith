using Microsoft.EntityFrameworkCore;
using Ultramaverick.Identity.Domain.Entities;
using Ultramaverick.Identity.Domain.ValueObjects;
using Ultramaverick.Identity.Persistence;

namespace Ultramaverick.Identity.IntegrationTests;

internal static class TestData
{
    public static readonly PasswordHash SampleHash =
        PasswordHash.FromEncoded("PBKDF2;SHA256;210000;AAAAAAAAAAAAAAAAAAAAAA==;AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

    /// <summary>
    /// Creates a role and a department (whose creation may raise events), clears the
    /// outbox, then creates a user so exactly the user's event remains in the outbox.
    /// Returns the new user id.
    /// </summary>
    public static async Task<int> CreateUserAsync(IdentityDbContext context, string? userName = null)
    {
        var role = Role.Create($"Role_{Guid.NewGuid():N}", null);
        var department = Department.Create($"Dept_{Guid.NewGuid():N}");
        context.Roles.Add(role);
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        await ClearOutboxAsync(context);

        var user = User.Create(
            "Test User",
            userName ?? $"user_{Guid.NewGuid():N}",
            SampleHash,
            role.Id,
            department.Id,
            null);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user.Id;
    }

    public static async Task ClearOutboxAsync(IdentityDbContext context)
    {
        context.OutboxMessages.RemoveRange(context.OutboxMessages);
        await context.SaveChangesAsync();
    }
}
