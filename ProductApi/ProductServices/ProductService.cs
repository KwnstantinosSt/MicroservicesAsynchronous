using System.Text.Json;
using Confluent.Kafka;
using Shared;

namespace ProductApi.ProductServices;

public interface IProductService
{
    Task AddProduct(Product product);
    Task DeleteProduct(int id);
}

public class ProductService : IProductService
{
    private static List<Product> _products = [];
    private readonly IProducer<Null, string> _producer;
    
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProducer<Null, string> producer, ILogger<ProductService> logger)
    {
        _producer = producer;
        _logger = logger;
    }
    
    public async Task AddProduct(Product product)
    {
        _products.Add(product);
        var result = await _producer.ProduceAsync("add-product-topic",
            new() { Value = JsonSerializer.Serialize(product) });
        
        _logger.LogInformation("Produced message to topic add-product-topic: {@result}", JsonSerializer.Serialize(result));
        _logger.LogInformation("Added Product: {@product}", JsonSerializer.Serialize(product));

        if (result.Status != PersistenceStatus.Persisted)
        {
            var lastProduct = _products.Last();
            _products.Remove(lastProduct);
            
            _logger.LogWarning("Delete Product: {@product} because it failed to produce the message {@result}", JsonSerializer.Serialize(product), JsonSerializer.Serialize(result));
        }
    }

    public async Task DeleteProduct(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        
        if (product == null) throw new ApplicationException("Product not found");
        
        _products.Remove(product);
        var result = await _producer.ProduceAsync("delete-product-topic",
            new() { Value = id.ToString() });
        
        _logger.LogInformation("Produced message to topic delete-product-topic {@result}", result);
        _logger.LogInformation("Deleted Product: {@product}", product);
    }
}