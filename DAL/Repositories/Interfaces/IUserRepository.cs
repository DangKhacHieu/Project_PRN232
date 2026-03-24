using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailOrPhoneAsync(string identifier);
        Task<User?> GetByIdAsync(int id);
        Task<bool> ExistsByEmailOrPhoneAsync(string email, string phone);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByPhoneAsync(string phone);
        Task<User> CreateAsync(User user);
        Task<VendorProfile> CreateVendorProfileAsync(VendorProfile profile);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);

        Task<ChangePasswordOtp> CreateOtpAsync(ChangePasswordOtp otp);
        Task<ChangePasswordOtp?> GetLatestOtpAsync(int userId);
        Task<bool> MarkOtpUsedAsync(int otpId);
    }
}
