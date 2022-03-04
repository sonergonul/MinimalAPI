// Minimal API

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<CustomerDb>(options => options.UseInMemoryDatabase("CustomerDb"));
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API V1");
});

app.MapGet("/hello", () => "Hello there");

app.MapGet
    ("/customers", 
    async (CustomerDb db) => await db.Customers.ToListAsync()
    );

app.MapGet(
    "/products",
    (int? pageNumber) => 
    $"Requesting page {pageNumber ?? 1}");

// BadHttpRequestException:
// Required parameter "int pageNumber"
// was not provided from query string.

string ListProducts(int pageNumber = 1) 
    => $"Requesting page {pageNumber}";

app.MapGet("/products", ListProducts);


app.MapPost("/customers", async (CustomerDb db, Customer cus) =>
{
    await db.Customers.AddAsync(cus);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{cus.Id}", cus);
});

app.MapGet("/customers/{id}", async (CustomerDb db, int id) => await db.Customers.FindAsync(id)).AllowAnonymous();

app.MapPut("/customers/{id}", async (CustomerDb db, Customer newCustomer, int id) =>
{
    var current = await db.Customers.FindAsync(id);

    if (current == null) return Results.NotFound();

    current.Name = newCustomer.Name;
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/customers/{id}", async (CustomerDb db, int id) =>
{
    var current = await db.Customers.FindAsync(id);

    if (current == null) return Results.NotFound();

    db.Customers.Remove(current);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();

class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
};

class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseInMemoryDatabase("CustomerDb");
    }
}

class Hello
{
    public static string Hi()
    {
        return "Hi!";
    }
}