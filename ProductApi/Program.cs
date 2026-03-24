using Confluent.Kafka;
using Microsoft.OpenApi;
using ProductApi.ProductServices;

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
builder.Services.AddSingleton<IProducer<Null, string>>(x => 
    new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = " 91.99.78.119:9094"})
        .Build());

// Services
builder.Services.AddScoped<IProductService, ProductService>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.UseForwardedHeaders();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();