using Microsoft.EntityFrameworkCore;
using CornerStore.Models;

public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed Cashiers
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier { Id = 1, FirstName = "John", LastName = "Doe" },
            new Cashier { Id = 2, FirstName = "Jane", LastName = "Smith" },
            new Cashier { Id = 3, FirstName = "Michael", LastName = "Brown" },
            new Cashier { Id = 4, FirstName = "Sarah", LastName = "Johnson" },
            new Cashier { Id = 5, FirstName = "Emily", LastName = "Davis" }
        });

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category { Id = 1, CategoryName = "Beverages" },
            new Category { Id = 2, CategoryName = "Snacks" },
            new Category { Id = 3, CategoryName = "Dairy" },
            new Category { Id = 4, CategoryName = "Bakery" },
            new Category { Id = 5, CategoryName = "Frozen Foods" }
        });

        // Seed Products
        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product { Id = 1, ProductName = "Cola", Price = 1.99M, Brand = "Brand A", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Orange Juice", Price = 2.99M, Brand = "Brand B", CategoryId = 1 },
            new Product { Id = 3, ProductName = "Chips", Price = 2.49M, Brand = "Brand C", CategoryId = 2 },
            new Product { Id = 4, ProductName = "Cheese", Price = 3.49M, Brand = "Brand D", CategoryId = 3 },
            new Product { Id = 5, ProductName = "Ice Cream", Price = 4.99M, Brand = "Brand E", CategoryId = 5 }
        });

        // Seed Orders
        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order { Id = 1, CashierId = 1, PaidOnDate = DateTime.Now.AddDays(-3) },
            new Order { Id = 2, CashierId = 2, PaidOnDate = DateTime.Now.AddDays(-2) },
            new Order { Id = 3, CashierId = 3, PaidOnDate = DateTime.Now.AddDays(-1) },
            new Order { Id = 4, CashierId = 4, PaidOnDate = DateTime.Now },
            new Order { Id = 5, CashierId = 5, PaidOnDate = null }
        });

        // Seed OrderProducts
        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
            new OrderProduct { OrderId = 1, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 2, Quantity = 3 },
            new OrderProduct { OrderId = 3, ProductId = 4, Quantity = 1 },
            new OrderProduct { OrderId = 4, ProductId = 5, Quantity = 2 }
        });

        modelBuilder.Entity<OrderProduct>().HasKey(op => new { op.OrderId, op.ProductId });
    }
}
