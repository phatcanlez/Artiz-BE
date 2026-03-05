using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] string? search)
    {
        try
        {
            IEnumerable<ProductDto> products;
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = await _productService.SearchProductsAsync(search);
            }
            else
            {
                products = await _productService.GetActiveProductsAsync();
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin sản phẩm" });
        }
    }
}

