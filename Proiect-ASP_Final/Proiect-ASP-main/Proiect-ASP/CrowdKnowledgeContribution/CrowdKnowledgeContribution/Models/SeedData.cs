using CrowdKnowledgeContribution.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrowdKnowledgeContribution.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any())
                {
                    return;
                }
                context.Roles.AddRange(
                new IdentityRole { Id = "b7149d10-9e20-49b6-acdc-cc5d11f2e3f7", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                new IdentityRole { Id = "81f135ef-4e75-441e-bd6c-16f7a0f0b96f", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                new IdentityRole { Id = "94577e84-a9f7-4156-9177-f3e84bf47719", Name = "Editor interzis", NormalizedName = "Editor interzis".ToUpper() }
                );
                var hasher = new PasswordHasher<ApplicationUser>();
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "e2321eb4-0db8-4386-bc33-10e2acc3b555",
                    UserName = "admin@gmail.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@GMAIL.COM",
                    Email = "admin@gmail.com",
                    NormalizedUserName = "ADMIN@GMAIL.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },
                new ApplicationUser
                {
                    Id = "4ca233f1-6271-46aa-8ff2-89551dd0838e",
                    UserName = "editor@gmail.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "EDITOR@GMAIL.COM",
                    Email = "editor@gmail.com",
                    NormalizedUserName = "EDITOR@GMAIL.COM",
                    PasswordHash = hasher.HashPassword(null, "Editor1!")
                },
                new ApplicationUser
                {
                    Id = "62f2f427-6f2a-4256-8d7c-c3128b9c6947",
                    UserName = "editor_interzis@gmail.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "EDITOR_INTERZIS@GMAIL.COM",
                    Email = "editor_interzis@gmail.com",
                    NormalizedUserName = "EDITOR_INTERZIS@GMAIL.COM",
                    PasswordHash = hasher.HashPassword(null, "Editor_interzis1!")
                }
                );
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "b7149d10-9e20-49b6-acdc-cc5d11f2e3f7",
                    UserId = "e2321eb4-0db8-4386-bc33-10e2acc3b555"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "81f135ef-4e75-441e-bd6c-16f7a0f0b96f",
                    UserId = "4ca233f1-6271-46aa-8ff2-89551dd0838e"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "94577e84-a9f7-4156-9177-f3e84bf47719",
                    UserId = "62f2f427-6f2a-4256-8d7c-c3128b9c6947"
                }
                );
                context.SaveChanges();
            }
        }
    }
}
