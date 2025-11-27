using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Atk.Data;
using Microsoft.EntityFrameworkCore;
using Atk.Services.Interfaces;
using Atk.Services.Implementations;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Tambahkan OpenAPI (.NET 8/9/10)
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ===============================
// Database
// ===============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ===============================
// Rate Limiting
// ===============================
builder.Services.AddRateLimiter(options =>
{
    // Supplier Bulk Limit (sudah ada)
    options.AddFixedWindowLimiter("supplier_bulk_limit", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 3;
        opt.QueueLimit = 0;
    });

    // Pengadaan Bulk Limit (TAMBAHAN)
    options.AddPolicy("pengadaan_bulk_limit", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // 5 request
                Window = TimeSpan.FromMinutes(1), // per 1 menit
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }
        )
    );
});

// ===============================
// Dependency Injection Services
// ===============================
builder.Services.AddScoped<ISupplierServices, SupplierService>();
builder.Services.AddScoped<IBarang, BarangService>();
builder.Services.AddScoped<IPengadaan, PengadaanService>();

// Tambahkan untuk BarangMasuk (TAMBAHAN)
builder.Services.AddScoped<IBarangMasuk, BarangMasukService>();

var app = builder.Build();

// ===============================
// Development Mode OpenAPI
// ===============================
if (app.Environment.IsDevelopment())
{
    // JSON OpenAPI
    app.MapOpenApi();

    app.MapControllers();

    // Swagger UI
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API Documentation V1");
        options.RoutePrefix = "swagger";
    });
}

// Aktifkan RateLimiter Middleware
app.UseRateLimiter();

// Routing
app.MapControllers();

app.Run();
