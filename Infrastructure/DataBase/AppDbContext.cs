using CatStoreAPI.Core.Models;
using Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.DataBase
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasOne(c => c.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(x => x.CategoryId);

            builder.Entity<ShoppingCart>()
                .HasMany(i => i.Items)
                .WithOne(s => s.ShoppingCart)
                .HasForeignKey(x => x.ShoppingCartId);

            builder.Entity<ShoppingCartItem>()
                .HasOne(p => p.Product)
                .WithMany(i => i.Items)
                .HasForeignKey(x => x.ProductId);

            builder.Entity<WishList>()
                .HasMany(w => w.Products)
                .WithMany(p => p.WhishLists)
            .   UsingEntity(j => j.ToTable("WishlistProducts"));

            builder.Entity<ShoppingCart>()
                .HasOne(u => u.User)
                .WithOne(s => s.ShoppingCart)
                .HasForeignKey<ShoppingCart>(x => x.userId);

            builder.Entity<WishList>()
                .HasOne(u => u.user)
                .WithOne(s => s.WishList)
                .HasForeignKey<WishList>(x => x.userId);

            builder.Entity<Product>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

            // User-Review relationship
            builder.Entity<AppUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ShoppingCartItem> Items { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
