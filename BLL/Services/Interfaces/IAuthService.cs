using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResult?> AuthenticateAsync(LoginRequest request);
        Task<RegisterResult> RegisterAsync(RegisterRequest request);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> SendChangePasswordOtpAsync(int userId, string email);
        Task<bool> ChangePasswordWithOtpAsync(int userId, string otp, string newPassword);
        Task<bool> VerifyChangePasswordOtpAsync(int userId, string otp);
    }
}