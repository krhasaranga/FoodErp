using System.Text;
using FoodErp.Api.Middleware;
using Microsoft.OpenApi.Models;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Admin;
using FoodErp.Application.Interfaces.Distribution;
using FoodErp.Application.Interfaces.Employees;
using FoodErp.Application.Interfaces.Inventory;
using FoodErp.Application.Interfaces.Invoicing;
using FoodErp.Application.Interfaces.Production;
using FoodErp.Application.Interfaces.Products;
using FoodErp.Application.Interfaces.Reports;
using FoodErp.Infrastructure.Identity;
using FoodErp.Infrastructure.Persistence;
using FoodErp.Infrastructure.Services.Admin;
using FoodErp.Infrastructure.Services.Distribution;
using FoodErp.Infrastructure.Services.Employees;
using FoodErp.Infrastructure.Services.Inventory;
using FoodErp.Infrastructure.Services.Invoicing;
using FoodErp.Infrastructure.Services.Production;
using FoodErp.Infrastructure.Services.Products;
using FoodErp.Infrastructure.Services.Reports;
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

// --- Identity (AddIdentityCore avoids cookie auth conflict with JWT) ---
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager();

// --- JWT Authentication ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdmin", policy => policy.RequireRole("PlatformAdmin"));
});

// --- Token service ---
builder.Services.AddScoped<TokenService>();

// --- Products module ---
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IProductService, ProductService>();

// --- Inventory module ---
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IStockService, StockService>();

// --- Production module ---
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

// --- Distribution module ---
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();

// --- Invoicing module ---
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// --- Employees module ---
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// --- Reports ---
builder.Services.AddScoped<IReportService, ReportService>();

// --- SaaS Administration ---
builder.Services.AddScoped<IAdminService, AdminService>();

// --- CORS — allow Next.js dev server ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// --- Framework services ---
builder.Services.AddControllers();

// --- Swagger / OpenAPI ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FoodERP API", Version = "v1" });

    // JWT Bearer auth — adds the green Authorize button
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here (without 'Bearer ' prefix)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodERP API v1"));
}

// CORS must come before UseHttpsRedirection
// Otherwise preflight OPTIONS requests get redirected before CORS headers are set
app.UseCors("FrontendDev");

// Only force HTTPS in production — in dev, HTTP stays on 5036 for the frontend proxy
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Global exception handler — must be first in the pipeline to catch all errors
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error is not null)
        {
            var status = error.Error is KeyNotFoundException ? 404 : 500;
            // In Development, return full exception detail for debugging
            var message = app.Environment.IsDevelopment()
                ? $"{error.Error.GetType().Name}: {error.Error.Message}\n{error.Error.StackTrace}"
                : error.Error is KeyNotFoundException
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