using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId)
        {
            // Lấy danh sách sản phẩm của vendor, có thể kèm theo Category nếu cần
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.PriceHistories)
                .Where(p => p.VendorId == vendorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAndVendorIdAsync(int productId, int vendorId)
        {
            // Đảm bảo chỉ lấy sản phẩm đúng của vendor đó (bảo mật dữ liệu)
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.PriceHistories)
                .FirstOrDefaultAsync(p => p.ProductId == productId && p.VendorId == vendorId);
        }

        public async Task CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}
