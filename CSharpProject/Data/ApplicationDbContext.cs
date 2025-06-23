using CSharpProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CSharpProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Data
            modelBuilder.Entity<Category>().HasData(
               new Category { CategoryId = 1, Name = "Keyboard" },
               new Category { CategoryId = 2, Name = "Mouse" },
               new Category { CategoryId = 3, Name = "Processor" },
               new Category { CategoryId = 4, Name = "Graphics Card" },
               new Category { CategoryId = 5, Name = "Motherboard" }
            );

            modelBuilder.Entity<Product>().HasData(

                new Product
                {
                    ProductId = 1,
                    Name = "Intel Core i9-13900K",
                    Description = "13th Gen, 24-core unlocked desktop processor",
                    Price = 599.99m,
                    Stock = 100,
                    CategoryId = 3
                },
                new Product
                {
                    ProductId = 2,
                    Name = "ASUS ROG Strix Z790-E",
                    Description = "Gaming motherboard with DDR5 and Wi-Fi 6E",
                    Price = 389.99m,
                    Stock = 120,
                    CategoryId = 5
                },
                new Product
                {
                    ProductId = 3,
                    Name = "ASUS TUF RX 7800 XT",
                    Description = "16GB GDDR6, PCIe 4.0 GPU for gaming",
                    Price = 499.99m,
                    Stock = 90,
                    CategoryId = 4
                }
            );
        }
    }
}
