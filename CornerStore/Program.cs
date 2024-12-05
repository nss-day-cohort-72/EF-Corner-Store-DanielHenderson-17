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