using AuthServiceAPI.Models;
using Microsoft.EntityFrameworkCore;
using AuthServiceAPI.Models;

namespace AuthServiceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users"); // lowercase table name
                entity.Property(u => u.Id).HasColumnName("id");
                entity.Property(u => u.Email).HasColumnName("email");
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
                entity.Property(u => u.PasswordSalt).HasColumnName("password_salt");
                entity.Property(u => u.FirstName).HasColumnName("first_name");
                entity.Property(u => u.LastName).HasColumnName("last_name");
                entity.Property(u => u.LastLogin).HasColumnName("last_login");
                entity.Property(u => u.FailedAttempts).HasColumnName("failed_attempts");
                entity.Property(u => u.AccountLocked).HasColumnName("account_locked");
                entity.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
                entity.Property(u => u.CreatedAt).HasColumnName("created_at");
                entity.Property(u => u.UpdatedAt).HasColumnName("updated_at");


                entity.HasIndex(u => u.Email).IsUnique(); // Unique email constraint
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configure the Session entity
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("sessions");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id).HasColumnName("id");
                entity.Property(s => s.UserId).HasColumnName("user_id");
                entity.Property(s => s.IpAddress).HasColumnName("ip_address");
                entity.Property(s => s.DeviceHash).HasColumnName("device_hash");
                entity.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.Property(s => s.ExpiresAt).HasColumnName("expires_at");
                entity.Property(s => s.LastAccessed).HasColumnName("last_accessed").HasDefaultValueSql("NOW()");
                entity.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}