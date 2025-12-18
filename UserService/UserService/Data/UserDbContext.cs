using Microsoft.EntityFrameworkCore;
using System.Net;
using UserService.Models;

namespace UserService.Data
{
    public class UserDbContext:DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
           : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserProfile
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(e => e.Country)
                    .HasMaxLength(500);

                // Inside the OnModelCreating method, update the CartItem entity configuration
                modelBuilder.Entity<CartItem>(entity =>
                {
                    entity.HasKey(e => e.Id);
                    entity.HasIndex(e => new { e.UserId, e.ProductId });

                    entity.HasOne(e => e.User)
                        .WithMany(u => u.CartItems)
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.Property(e => e.ProductId)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.Size)
                        .HasMaxLength(10);

                    entity.Property(e => e.Price)
                        .HasPrecision(18, 2); // Replace HasColumnType with HasPrecision
                });
                entity.Property(e => e.City)
                    .HasMaxLength(500);
            });

            

            // WishlistItem
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(u => u.WishlistItems)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.ProductId });

                entity.HasOne(e => e.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Size)
                    .HasMaxLength(10);

                entity.Property(e => e.Price)
                    .HasPrecision(18,2);
            });


            // Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
