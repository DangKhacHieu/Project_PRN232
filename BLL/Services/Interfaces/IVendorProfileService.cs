using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IVendorProfileService
    {
        Task<VendorProfileResponseDTO> GetProfileAsync(int vendorId);
        Task<bool> UpdateDescriptionAsync(int vendorId, VendorProfileUpdateRequestDTO request);
    }
}
