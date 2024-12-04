using Google.Api;
using heroTopCollectionAPI.Data.Context;
using heroTopCollectionAPI.Middlewares;
using heroTopCollectionAPI.Services;
using heroTopCollectionAPI.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 1048576; // 1 MB (puedes ajustarlo según lo que necesites)
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<FirestoreContext>();
builder.Services.AddSingleton<StorageContext>();
builder.Services.AddSingleton<AuthenticationContext>();
builder.Services.AddSingleton<CreateToken>();
builder.Services.AddSingleton<HashPassword>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductsService>();

// Enable CORS with any origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin() // Permite cualquier origen
               .AllowAnyMethod() // Permite cualquier método (GET, POST, etc.)
               .AllowAnyHeader(); // Permite cualquier encabezado
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add the CORS middleware here
app.UseCors();  // Habilita CORS con la política definida previamente

app.UseRouting();

app.UseMiddleware<VerifyRequest>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
