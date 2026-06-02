using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Paylite API",
        Version = "v1",
        Description = "Payment processing API with idempotency, webhook verification, and audit logging."
    });
});

var app = builder.Build();

app.UseSwagger(options =>
{
    options.RouteTemplate = "v3/api-docs/{documentName}.json";
});

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger-ui";
    options.SwaggerEndpoint("/v3/api-docs/v1.json", "Paylite API v1");
});

app.MapGet("/swaggr-ui/{**path}", context =>
{
    var path = context.Request.RouteValues["path"]?.ToString();
    context.Response.Redirect(string.IsNullOrWhiteSpace(path) ? "/swagger-ui" : $"/swagger-ui/{path}");
    return Task.CompletedTask;
}).AllowAnonymous();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
