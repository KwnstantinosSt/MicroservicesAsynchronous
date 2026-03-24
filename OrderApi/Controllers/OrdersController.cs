using Microsoft.AspNetCore.Mvc;
using OrderApi.OrderServices;
using Shared;

namespace OrderApi.Controllers;

[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("Consume")]
    public async Task<IActionResult> Consume()
    {
        await _orderService.StartConsumingService();
        return NoContent();
    }
    
    [HttpGet("Products")]
    public async Task<IActionResult> Products()
    {
        var products = _orderService.GetProducts();
        return Ok(products);
    }
    
    [HttpGet("Summary")]
    public async Task<IActionResult> Summary()
    {
        var products = _orderService.GetOrdersSummary();
        return Ok(products);
    }
    
    [HttpPost]
    public async Task<IActionResult> Order([FromBody] Order order)
    {
        _orderService.AddOrder(order);
        return Created();
    }
}