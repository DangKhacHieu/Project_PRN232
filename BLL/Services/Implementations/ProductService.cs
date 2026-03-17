using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace BLL.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;

        public ProductService(IProductRepository productRepo)
        {
            _productRepo = productRepo;
        }

        // 1. LẤY DANH SÁCH
        public async Task<IEnumerable<ProductResponseDTO>> GetProductsByVendorAsync(int vendorId)
        {
            var products = await _productRepo.GetByVendorIdAsync(vendorId);

            // Map Entity -> DTO (Nếu dự án dùng AutoMapper thì sẽ gọn hơn rất nhiều)
            return products.Select(p => new ProductResponseDTO
            {
                ProductId = p.ProductId,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
                ProductName = p.ProductName,
                Unit = p.Unit,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        // 2. CẬP NHẬT
        public async Task<bool> UpdateProductAsync(int vendorId, int productId, ProductUpdateRequestDTO request)
        {
            var product = await _productRepo.GetByIdAndVendorIdAsync(productId, vendorId);
            if (product == null) return false;

            product.ProductName = request.ProductName;
            product.CategoryId = request.CategoryId;
            product.Unit = request.Unit;
            product.IsActive = request.IsActive;

            // Xử lý lưu ảnh mới nếu có (Ví dụ đơn giản, thực tế cần lưu file vào wwwroot/images)
            if (request.NewImage != null)
            {
                // Đoạn logic lưu file IFormFile vào server và lấy ra đường dẫn URL
                // product.ImageUrl = ".../images/" + fileName; 
            }

            await _productRepo.UpdateAsync(product);
            return true;
        }

        // 3. XÓA MỀM (NGỪNG BÁN)
        public async Task<bool> DeleteProductAsync(int vendorId, int productId)
        {
            var product = await _productRepo.GetByIdAndVendorIdAsync(productId, vendorId);
            if (product == null) return false;

            product.IsActive = false; // Soft delete
            await _productRepo.UpdateAsync(product);
            return true;
        }

        // 4. THÊM MỚI (Đã có từ trước, nay chuyển sang dùng Repo)
        public async Task<int> CreateProductAsync(int vendorId, ProductCreateRequestDTO request)
        {
            var newProduct = new Product
            {
                VendorId = vendorId,
                ProductName = request.ProductName,
                CategoryId = request.CategoryId,
                Unit = request.Unit,
                //ImageUrl = request.ImageUrl, chua code
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _productRepo.CreateAsync(newProduct);
            return newProduct.ProductId;
        }
    }
}
