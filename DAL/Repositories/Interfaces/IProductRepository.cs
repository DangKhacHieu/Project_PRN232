using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId);
        Task<Product?> GetByIdAndVendorIdAsync(int productId, int vendorId);
        Task CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task<IEnumerable<ProductCategory>> GetCategoriesAsync();
    }
}
