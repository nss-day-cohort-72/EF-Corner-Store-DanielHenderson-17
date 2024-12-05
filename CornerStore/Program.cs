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






app.Run();

//don't move or change this!
public partial class Program { }