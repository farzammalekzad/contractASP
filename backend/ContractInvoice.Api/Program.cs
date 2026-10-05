using ContractInvoice.Api.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapGet("/api/test", () =>
{
    return Results.Ok(new {message = "Hello"});
})
.WithName("TEST");

app.MapPost("/api/contracts", (Contract contract) =>
{
    return Results.Ok(contract);
});

app.Run();
