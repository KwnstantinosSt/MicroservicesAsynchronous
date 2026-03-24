using Confluent.Kafka;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;
using OrderApi.OrderServices;

var builder = WebApplication.CreateBuilder(args);

// Add Configuration Files
builder.Configuration.SetBasePath(Path.Combine(builder.Environment.ContentRootPath, "Configurations"));
builder.Configuration
    .AddJsonFile(appsettings =>
    {
        appsettings.Optional = false;
        appsettings.Path = $"appsettings.{builder.Environment.EnvironmentName}.json";
        appsettings.ReloadOnChange = true;
    });

// App Services
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
});

// Add Infrastructure
builder.Services.AddSingleton<IConsumer<Null, string>>(x => 
    new ConsumerBuilder<Null, string>(new ConsumerConfig
        {
            BootstrapServers = " 91.99.78.119:9094",
            GroupId = "add-product-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        })
        .Build());

// Services
builder.Services.AddScoped<IOrderService, OrderService>();

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Forward headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    // CRITICAL: Clear these so it trusts the Coolify/Docker proxy
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();