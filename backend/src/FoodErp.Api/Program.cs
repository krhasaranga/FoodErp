using System.Text;
using FoodErp.Api.Middleware;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Products;
using FoodErp.Infrastructure.Identity;
using FoodErp.Infrastructure.Persistence;
using FoodErp.Infrastructure.Services.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Tenant context (scoped: one per HTTP request) ---
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

// --- Identity ---
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Relax password rules slightly for development; tighten for production
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// --- Authorization ---
builder.Services.AddAuthorization();

// --- Token service ---
builder.Services.AddScoped<TokenService>();

// --- Products module ---
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();

// --- Framework services ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Global exception handler — must be first in the pipeline to catch all errors
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error is not null)
        {
            var status = error.Error is KeyNotFoundException ? 404 : 500;
            var message = error.Error is KeyNotFoundException
                ? error.Error.Message
                : "An unexpected error occurred.";
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = message });
        }
    });
});

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution runs after auth so we can read JWT claims
app.UseMiddleware<TenantResolutionMiddleware>();

app.MapControllers();

app.Run();