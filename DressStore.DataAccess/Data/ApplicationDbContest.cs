using DressStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DressStore.DataAccess.Data
{
    public class ApplicationDbContest : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContest(DbContextOptions<ApplicationDbContest> options) : base(options)
        {
            
        }

        public DbSet<Category> categories { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ShoppingCart> shoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeader { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Shirt", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Hoodies", DisplayOrder = 2 },
                new Category { Id = 3, Name = "T-Shirt", DisplayOrder = 3 }
                );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Title ="Levis" , Description = "Checked Hooded Jacket", Price = 1000 , CategoryId =1 , ImageUrl=""},
                new Product { Id = 2, Title = "Peter England", Description = "Checked White&Blue Shirt", Price = 1210, CategoryId =2 , ImageUrl = "" },
                new Product { Id = 3, Title = "OTTO", Description = "Denim Shirt", Price = 900 , CategoryId = 3, ImageUrl = "" }
                );
        }

    }
}
