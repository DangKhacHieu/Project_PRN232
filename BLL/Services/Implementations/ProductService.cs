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
        private readonly IPhotoService _photoService;

        public ProductService(IProductRepository productRepo, IPhotoService photoService)
        {
            _productRepo = productRepo;
            _photoService = photoService;
        }

        // 1. LẤY DANH SÁCH
        public async Task<IEnumerable<ProductResponseDTO>> GetProductsByVendorAsync(int vendorId, int? categoryId = null, bool? isActive = null, string? search = null)
        {
            var products = await _productRepo.GetByVendorIdAsync(vendorId);

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }
            if (isActive.HasValue)
            {
                products = products.Where(p => p.IsActive == isActive.Value);
            }
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            // Map Entity -> DTO (Nếu dự án dùng AutoMapper thì sẽ gọn hơn rất nhiều)
            return products.Select(p => new ProductResponseDTO
            {
                ProductId = p.ProductId,
                VendorId = p.VendorId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                ProductName = p.ProductName,
                Unit = p.Unit,
                Price = p.PriceHistories.OrderByDescending(ph => ph.EffectiveTime).FirstOrDefault()?.Price ?? 0,
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

            // Xử lý giá bán mới (thêm vào lịch sử nếu thay đổi)
            var currentPrice = product.PriceHistories.OrderByDescending(ph => ph.EffectiveTime).FirstOrDefault()?.Price ?? 0;
            if (request.Price != currentPrice)
            {
                product.PriceHistories.Add(new PriceHistory
                {
                    Price = request.Price,
                    EffectiveTime = DateTime.Now
                });
            }

            // Xử lý lưu ảnh mới nếu có
            if (request.NewImage != null)
            {
                var uploadedUrl = await _photoService.AddPhotoAsync(request.NewImage, "products");
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    product.ImageUrl = uploadedUrl;
                }
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
                IsActive = true,
                CreatedAt = DateTime.Now,
                PriceHistories = new List<PriceHistory>
                {
                    new PriceHistory
                    {
                        Price = request.Price,
                        EffectiveTime = DateTime.Now
                    }
                }
            };

            if (request.Image != null)
            {
                var uploadedUrl = await _photoService.AddPhotoAsync(request.Image, "products");
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    newProduct.ImageUrl = uploadedUrl;
                }
            }

            await _productRepo.CreateAsync(newProduct);
            return newProduct.ProductId;
        }
    }
}
