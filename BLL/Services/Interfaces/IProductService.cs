using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<int> CreateProductAsync(int vendorId, ProductCreateRequestDTO request);
        Task<IEnumerable<ProductResponseDTO>> GetProductsByVendorAsync(int vendorId);
        Task<bool> UpdateProductAsync(int vendorId, int productId, ProductUpdateRequestDTO request);
        Task<bool> DeleteProductAsync(int vendorId, int productId);
    }
}
