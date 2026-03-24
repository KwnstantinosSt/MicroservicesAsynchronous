using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Shared;

namespace OrderApi.OrderServices;

public interface IOrderService
{
    Task StartConsumingService();
    void AddOrder(Order order);
    List<Product> GetProducts();
    List<OrderSummary> GetOrdersSummary();
}

public class OrderService : IOrderService
{
    private readonly IConsumer<Null, string> _consumer;
    
    private const string AddProductTopic = "add-product-topic";
    private const string DeleteProductTopic = "delete-product-topic";

    private static List<Order> _orders = [];
    private static List<Product> _products = [];
    
    private readonly ILogger<OrderService> _logger;

    public OrderService(IConsumer<Null, string> consumer, ILogger<OrderService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    public async Task StartConsumingService()
    {
        await Task.Delay(1000);
        _consumer.Subscribe([AddProductTopic, DeleteProductTopic]);

        _ = Task.Run(() =>
        {
            while (true)
            {
                var response = _consumer.Consume();
                _logger.LogInformation("Consuming message: {@Message}", JsonSerializer.Serialize(response));

                if (!string.IsNullOrEmpty(response.Message.Value))
                {
                    if (response.Topic == AddProductTopic)
                    {
                        var product = JsonSerializer.Deserialize<Product>(response.Message.Value);

                        if (product != null)
                        {
                            _products.Add(product);
                            _logger.LogInformation("Added product {@product}.", JsonSerializer.Serialize(product));
                        }
                    }
                
                    if (response.Topic == DeleteProductTopic)
                    {
                        var product = _products.FirstOrDefault(p => p.Id == int.Parse(response.Message.Value));

                        if (product != null)
                        {
                            _products.Remove(product);
                            _logger.LogInformation("Removed product {@product}.", JsonSerializer.Serialize(product));
                        }
                    }
                }
            }
        });
    }

    public void AddOrder(Order order)
        => _orders.Add(order);

    public List<Product> GetProducts()
        => _products;

    public List<OrderSummary> GetOrdersSummary()
    {
        var productsMap = _products.ToDictionary(k => k.Id);
        var ordersSummary = _orders
            .Where(o => productsMap.ContainsKey(o.ProductId))
            .Select(o => new OrderSummary
            {
                OrderId = o.Id,
                ProductId = o.ProductId,
                OrderedQuantity = o.Quantity,
                ProductName = productsMap[o.ProductId].Name,
                ProductPrice = productsMap[o.ProductId].Price,
            })
            .ToList();
        
        return ordersSummary;
    }
}