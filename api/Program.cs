using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using api.Data;
using api.Options;
using api.Interfaces;
using api.Services;
using System.Text.Json.Serialization;
using api.Security;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SecurityOptions>(
    builder.Configuration.GetSection(SecurityOptions.SectionName));
builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetSection(PaymentOptions.SectionName));

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddAuthentication(ApiKeyAuthenticationDefaults.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.SchemeName, _ => { });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    dbContext.Database.Migrate();
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
