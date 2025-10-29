using DeepHumans.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeepHumans.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Set all string primary keys to varchar(255) for MySQL
            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).HasMaxLength(255);
                entity.Property(r => r.NormalizedName).HasMaxLength(255);
            });

            builder.Entity<IdentityUser>(entity =>
            {
                entity.Property(u => u.Id).HasMaxLength(255);
                entity.Property(u => u.NormalizedEmail).HasMaxLength(255);
                entity.Property(u => u.NormalizedUserName).HasMaxLength(255);
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(l => l.LoginProvider).HasMaxLength(255);
                entity.Property(l => l.ProviderKey).HasMaxLength(255);
            });

            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(r => r.UserId).HasMaxLength(255);
                entity.Property(r => r.RoleId).HasMaxLength(255);
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(t => t.LoginProvider).HasMaxLength(255);
                entity.Property(t => t.Name).HasMaxLength(255);
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.Property(c => c.Id).HasMaxLength(255);
                entity.Property(c => c.RoleId).HasMaxLength(255);
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.Property(c => c.Id).HasMaxLength(255);
                entity.Property(c => c.UserId).HasMaxLength(255);
            });

            // ✅ Global charset for multilingual + emoji support
            builder.UseCollation("utf8mb4_general_ci");
            builder.HasCharSet("utf8mb4");
        }
    }
}
