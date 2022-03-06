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


// Minimal API - Responses

// IResult - Task<IResult> veya ValueTask<IResult>
// string - Task<string> veya ValueTask<string> - text/plain
// T - Json seralize - application/json

app.MapGet("/response1", () => "Hello World");
app.MapGet("/response2", () => new { Message = "Hello World" });
app.MapGet("/response3", () => Results.Ok(new { Message = "Hello World" }));
app.MapGet("/response4", () => Results.Json(new { Message = "Hello World" }));
app.MapGet("/405", () => Results.StatusCode(405));
app.MapGet("/text", () => Results.Text("This is some text"));
app.MapGet("/old-path", () => Results.Redirect("/new-path"));
app.MapGet("/download", () => Results.File("C:/appsettings.json", contentType: "application/json", fileDownloadName: "appsettings.json"));

app.MapGet
    ("/customers", 
    async (CustomerDb db) => await db.Customers.ToListAsync()
    );

//app.MapGet(
//    "/products",
//    (int? pageNumber) => 
//    $"Requesting page {pageNumber ?? 1}");

// BadHttpRequestException:
// Required parameter "int pageNumber"
// was not provided from query string.

string ListProducts(int pageNumber = 1) 
    => $"Requesting page {pageNumber}";

app.MapGet("/products", ListProducts);

// Http Options ve Http Head kullanýmý
// app.MapMethods("/options-or-head",
//    new[] { "OPTIONS", "HEAD" },
//    () => "This is an options or head request");


app.MapPost("/customers", async (CustomerDb db, Customer cus) =>
{
    await db.Customers.AddAsync(cus);
    await db.SaveChangesAsync();
    return Results.Created($"/customers/{cus.Id}", cus);
});

app.MapGet("/customers/{id}", async (CustomerDb db, int id) => await db.Customers.FindAsync(id));

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

// Port numarasýný deðiþtirmek
// app.Run("http://localhost:3000");

// Çoklu port kullanýmý
// app.Urls.Add("http://localhost:3000");
// app.Urls.Add("http://localhost:4000");

// Loglama
// app.Logger.LogInformation("Bu bir info");
// app.Logger.LogError("Bu bir error");
// app.Logger.LogWarning("Bu bir warning");
// app.Logger.LogCritical("Bu bir kritik log");

// appsettings.json dosyasýný okuma
// app.Logger.LogInformation($"{app.Configuration["JWT:Key"]}");

/// <summary>
/// Inmemory veritabaný için kullanýlacak müþteri model
/// </summary>
class Customer
{
    /// <summary>
    /// Müþteri id'si
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Müþteri ismi
    /// </summary>
    public string? Name { get; set; }
};

/// <summary>
/// Inmemory içerisinde kullanýlabilecek baðlantý sýnýfý
/// </summary>
class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseInMemoryDatabase("CustomerDb");
    }
}

//  Route Handler formatlarý
//app.MapGet("/", Local);
//Hello h = new Hello();
//app.MapGet("/", h.InstanceMethod);
//app.MapGet("/", Hello.StaticMethod);

//var lambda = () => "This is a lambda variable";

//app.MapGet("/", lambda);
//string Local() => "Soner";

//class Hello
//{
//    public string InstanceMethod()
//    {
//        return "Soner";
//    }

//    public static string StaticMethod()
//    {
//        return "Soner";
//    }
//}

// Route isimlendirme ve LinkGenerator
//app.MapGet("/hello", () => "Hello there")
//    .WithName("hi");

//app.MapGet("/", (LinkGenerator linker) =>
//@$"The link to the hello route is 
//{
//    linker.GetPathByName("hi", values: null)
//}");

// Route parametreleri
//app.MapGet("users/{username}/books/{id}",
//    (string username, int id)
//    => $"User: {username} Book: {id}");

// Opsiyonel parametreler
//app.MapGet(
//    "/products",
//    (int ? pageNumber) =>
//    $"Requesting page {pageNumber ?? 1}");

// Açýk olarak parametre binding
//app.MapGet(
//    "/{id}",
//    ([FromRoute] int id,
//     [FromQuery(Name = "p")] int page,
//     [FromServices] Service service,
//     [FromHeader(Name = "Content-Type")] string contentType) =>
//{ });

