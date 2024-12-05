using CornerStore.Models;
using CornerStore.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

//Get a cashier (include their orders, and the orders' products)
app.MapGet("/api/cashiers/{id}", async (int id, CornerStoreDbContext dbContext) =>
{

    var cashierDetails = await dbContext.Cashiers
        .Where(c => c.Id == id)
        .Select(c => new
        {
            c.Id,
            c.FirstName,
            c.LastName,
            FullName = $"{c.FirstName} {c.LastName}",
            Orders = c.Orders.Select(o => new
            {
                o.Id,
                o.PaidOnDate,
                o.Total,
                Products = o.OrderProducts.Select(op => new
                {
                    op.Product.Id,
                    op.Product.ProductName,
                    op.Product.Brand,
                    op.Product.Price,
                    op.Quantity
                }).ToList()
            }).ToList()
        })
        .FirstOrDefaultAsync();

    if (cashierDetails == null)
    {
        return Results.NotFound(new { Message = $"Cashier with ID {id} not found." });
    }

    return Results.Ok(cashierDetails);
});

//Get all products with categories. If the search query string param is present, return only products whose names or category names include the search value (ignore case).
app.MapGet("/api/products", async (string? search, CornerStoreDbContext dbContext) =>
{
    var products = dbContext.Products
        .Include(p => p.Category)
        .Select(p => new
        {
            p.Id,
            p.ProductName,
            p.Price,
            p.Brand,
            Category = p.Category.CategoryName
        });

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.ToLower();
        products = products.Where(p =>
            p.ProductName.ToLower().Contains(search) ||
            p.Category.ToLower().Contains(search));
    }

    return Results.Ok(await products.ToListAsync());
});

//Get an order details, including the cashier, order products, and products on the order with their category.
app.MapGet("/api/orders/{id}", async (int id, CornerStoreDbContext dbContext) =>
{

    var order = await dbContext.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound(new { Message = $"Order with ID {id} not found." });
    }

    var response = new
    {
        OrderId = order.Id,
        order.PaidOnDate,
        order.Total,
        Cashier = new
        {
            order.Cashier.Id,
            order.Cashier.FullName
        },
        Products = order.OrderProducts.Select(op => new
        {
            op.Product.Id,
            op.Product.ProductName,
            op.Product.Price,
            op.Quantity,
            Category = op.Product.Category.CategoryName
        }).ToList()
    };

    return Results.Ok(response);
});

//Get all orders. Check for a query string param orderDate that only returns orders from a particular day. If it is not present, return all orders.
app.MapGet("/api/orders", async (DateTime? orderDate, CornerStoreDbContext dbContext) =>
{
    var query = dbContext.Orders
        .Include(o => o.Cashier)
        .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category)
        .AsQueryable();

    if (orderDate.HasValue)
    {
        query = query.Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == orderDate.Value.Date);
    }

    var orders = await query.Select(o => new
    {
        o.Id,
        o.PaidOnDate,
        o.Total,
        Cashier = new
        {
            o.Cashier.Id,
            o.Cashier.FullName
        },
        Products = o.OrderProducts.Select(op => new
        {
            op.Product.Id,
            op.Product.ProductName,
            op.Quantity,
            op.Product.Price,
            Category = op.Product.Category.CategoryName
        }).ToList()
    }).ToListAsync();

    return Results.Ok(orders);
});


////Post Endpoints

//Add a cashier
app.MapPost("/api/cashiers", async (CashierDTO cashierDTO, CornerStoreDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(cashierDTO.FirstName) || string.IsNullOrWhiteSpace(cashierDTO.LastName))
    {
        return Results.BadRequest(new { Message = "FirstName and LastName are required." });
    }

    var cashier = new Cashier
    {
        FirstName = cashierDTO.FirstName,
        LastName = cashierDTO.LastName
    };

    dbContext.Cashiers.Add(cashier);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/cashiers/{cashier.Id}", new
    {
        cashier.Id,
        cashier.FirstName,
        cashier.LastName,
        FullName = $"{cashier.FirstName} {cashier.LastName}"
    });
});

//Add a product
app.MapPost("/api/products", async (ProductDTO productDTO, CornerStoreDbContext dbContext) =>
{

    if (string.IsNullOrWhiteSpace(productDTO.ProductName) ||
        string.IsNullOrWhiteSpace(productDTO.Brand) ||
        productDTO.Price <= 0 ||
        productDTO.CategoryId <= 0)
    {
        return Results.BadRequest(new { Message = "Invalid product data. Please check the input fields." });
    }

    var category = await dbContext.Categories.FindAsync(productDTO.CategoryId);
    if (category == null)
    {
        return Results.NotFound(new { Message = $"Category with ID {productDTO.CategoryId} not found." });
    }

    var product = new Product
    {
        ProductName = productDTO.ProductName,
        Price = productDTO.Price,
        Brand = productDTO.Brand,
        CategoryId = productDTO.CategoryId
    };


    dbContext.Products.Add(product);
    await dbContext.SaveChangesAsync();


    return Results.Created($"/api/products/{product.Id}", new
    {
        product.Id,
        product.ProductName,
        product.Price,
        product.Brand,
        Category = category.CategoryName
    });
});


//Create an Order (with products!)
app.MapPost("/api/orders", async (OrderCreateDTO orderDTO, CornerStoreDbContext dbContext) =>
{
    var cashier = await dbContext.Cashiers.FindAsync(orderDTO.CashierId);
    if (cashier == null)
    {
        return Results.NotFound(new { Message = $"Cashier with ID {orderDTO.CashierId} not found." });
    }

    var productIds = orderDTO.Products.Select(p => p.ProductId).ToList();
    var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

    if (products.Count != productIds.Count)
    {
        return Results.BadRequest(new { Message = "Some products in the order do not exist." });
    }

    var order = new Order
    {
        CashierId = orderDTO.CashierId,
        PaidOnDate = orderDTO.PaidOnDate,
        OrderProducts = orderDTO.Products.Select(op => new OrderProduct
        {
            ProductId = op.ProductId,
            Quantity = op.Quantity
        }).ToList()
    };

    dbContext.Orders.Add(order);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/orders/{order.Id}", new
    {
        OrderId = order.Id,
        Cashier = new { cashier.Id, FullName = cashier.FullName },
        order.PaidOnDate,
        Products = order.OrderProducts.Select(op => new
        {
            op.Product.Id,
            op.Product.ProductName,
            op.Quantity,
            op.Product.Price,
            TotalPrice = op.Quantity * op.Product.Price
        }).ToList(),
        order.Total
    });
});

////Put Endpoints

//Update a product
app.MapPut("/api/products/{id}", async (int id, Product productInput, CornerStoreDbContext dbContext) =>
{

    var product = await dbContext.Products.FindAsync(id);
    if (product == null)
    {
        return Results.NotFound(new { Message = $"Product with ID {id} not found." });
    }

    var category = await dbContext.Categories.FindAsync(productInput.CategoryId);
    if (category == null)
    {
        return Results.NotFound(new { Message = $"Category with ID {productInput.CategoryId} not found." });
    }

    product.ProductName = productInput.ProductName;
    product.Price = productInput.Price;
    product.Brand = productInput.Brand;
    product.CategoryId = productInput.CategoryId;

    await dbContext.SaveChangesAsync();

    return Results.Ok(product);
});


////Delete Endpoints

//Delete an order
app.MapDelete("/api/orders/{id}", async (int id, CornerStoreDbContext dbContext) =>
{

    var order = await dbContext.Orders
        .Include(o => o.OrderProducts)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return Results.NotFound(new { Message = $"Order with ID {id} not found." });
    }

    dbContext.OrderProducts.RemoveRange(order.OrderProducts);

    dbContext.Orders.Remove(order);

    await dbContext.SaveChangesAsync();

    return Results.Ok(new { Message = $"Order with ID {id} successfully deleted." });
});


app.Run();

//don't move or change this!
public partial class Program { }