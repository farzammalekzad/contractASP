using ContractInvoice.Api.Model;
using ContractInvoice.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PrincipalService>();
builder.Services.AddScoped<ContractAttachmentStorageService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<IPaymentDocumentService, PaymentDocumentService>();
builder.Services.AddScoped<IProgressPaymentService, ProgressPaymentService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () =>
{
    return Results.Ok(new { status = "ok" });
})
.WithName("HealthCheck");

app.UseRouting();
app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapControllers();

app.Run();
