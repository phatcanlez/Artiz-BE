using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BO.DTOs;
using BLL.Services;

namespace ArtizBackend.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<AdminProductsController> _logger;

    public AdminProductsController(IProductService productService, ILogger<AdminProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin products");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách sản phẩm" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductCreateUpdateDto request)
    {
        try
        {
            var created = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProducts), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo sản phẩm" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] ProductCreateUpdateDto request)
    {
        try
        {
            var updated = await _productService.UpdateProductAsync(id, request);
            if (updated == null)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật sản phẩm" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa sản phẩm" });
        }
    }
}


