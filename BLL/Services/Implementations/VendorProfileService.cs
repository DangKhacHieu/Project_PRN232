using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Repositories.Interfaces;

namespace BLL.Services.Implementations
{
    public class VendorProfileService : IVendorProfileService
    {
        private readonly IVendorProfileRepository _vendorProfileRepo;
        private readonly IPhotoService _photoService;

        public VendorProfileService(IVendorProfileRepository vendorProfileRepo, IPhotoService photoService)
        {
            _vendorProfileRepo = vendorProfileRepo;
            _photoService = photoService;
        }

        public async Task<VendorProfileResponseDTO> GetProfileAsync(int vendorId)
        {
            var profile = await _vendorProfileRepo.GetByVendorIdAsync(vendorId);
            if (profile == null) return null;

            return new VendorProfileResponseDTO
            {
                VendorId = profile.VendorId,
                BusinessName = profile.BusinessName,
                Description = profile.Description,
                CoverImageUrl = profile.CoverImageUrl,
                OwnerName = "Chủ gian hàng", 
                StallCode = profile.StallContracts?.FirstOrDefault(sc => sc.Status == "ACTIVE")?.Stall?.StallCode,
                LastUpdated = DateTime.Now
            };
        }

        public async Task<bool> UpdateDescriptionAsync(int vendorId, VendorProfileUpdateRequestDTO request)
        {
            var profile = await _vendorProfileRepo.GetByVendorIdAsync(vendorId);
            if (profile == null) return false;

            if (request.BusinessName != null)
                profile.BusinessName = request.BusinessName;
                
            if (request.Description != null)
                profile.Description = request.Description;

            if (request.NewCoverImage != null)
            {
                var uploadedUrl = await _photoService.AddPhotoAsync(request.NewCoverImage, "vendor_profiles");
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    profile.CoverImageUrl = uploadedUrl;
                }
            }

            await _vendorProfileRepo.UpdateAsync(profile);
            return true;
        }
    }
}
