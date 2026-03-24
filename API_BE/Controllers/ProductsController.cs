using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateRequestDTO request)
        {
            // 1. Kiểm tra dữ liệu đầu vào (Validation)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 2. TRÍCH XUẤT VENDOR ID TỪ JWT TOKEN
            int vendorId = GetVendorIdFromToken(); 
            if (vendorId <= 0) return Unauthorized();

            // 3. Gọi Service xử lý
            try
            {
                int productId = await _productService.CreateProductAsync(vendorId, request);

                return Ok(new
                {
                    Success = true,
                    Message = "Thêm sản phẩm thành công!",
                    ProductId = productId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProducts([FromQuery] int? categoryId, [FromQuery] bool? isActive, [FromQuery] string? search)
        {
            int vendorId = GetVendorIdFromToken(); 
            if (vendorId <= 0) return Unauthorized();


            var products = await _productService.GetProductsByVendorAsync(vendorId, categoryId, isActive, search);
            return Ok(products);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductUpdateRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var isSuccess = await _productService.UpdateProductAsync(vendorId, id, request);
            if (!isSuccess) return NotFound(new { Message = "Không tìm thấy sản phẩm hoặc bạn không có quyền sửa." });

            return Ok(new { Success = true, Message = "Cập nhật thành công!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int vendorId = GetVendorIdFromToken();
            if (vendorId <= 0) return Unauthorized();

            var isSuccess = await _productService.DeleteProductAsync(vendorId, id);
            if (!isSuccess) return NotFound(new { Message = "Không tìm thấy sản phẩm." });

            return Ok(new { Success = true, Message = "Đã gỡ sản phẩm thành công!" });
        }

        // Hàm helper dùng chung trong Controller này
        private int GetVendorIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type.Equals("VendorId", StringComparison.OrdinalIgnoreCase) || c.Type.Equals("vendor_id", StringComparison.OrdinalIgnoreCase));
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
    }
}