using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services.Implementations
{
    public class VendorService : IVendorService
    {
        private readonly IVendorRepository _vendorRepo;
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private const string PasswordChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#";

        public VendorService(IVendorRepository vendorRepo, IUserRepository userRepository)
        {
            _vendorRepo = vendorRepo;
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<List<VendorDTO>> GetAllVendorsAsync()
        {
            var users = await _vendorRepo.GetAllVendorsAsync();

            return users.Select(u => new VendorDTO
            {
                VendorId = u.UserId,
                BusinessName = u.VendorProfile?.BusinessName ?? u.FullName,
                CreatedAt = u.CreatedAt,
                User = new VendorUserDetailDTO
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone
                }
            }).ToList();
        }

        public async Task<AdminResetPasswordPreviewResultDTO> PreviewAdminResetPasswordAsync(int vendorId)
        {
            var user = await _vendorRepo.GetVendorByIdAsync(vendorId);
            if (user == null) throw new Exception("Không tìm thấy tiểu thương này trong hệ thống!");

            return new AdminResetPasswordPreviewResultDTO
            {
                VendorId = user.UserId,
                BusinessName = user.VendorProfile?.BusinessName ?? user.FullName,
                ContactName = user.FullName,
                Email = user.Email,
                NewPassword = GenerateRandomPassword(10)
            };
        }

        public async Task ConfirmAdminResetPasswordAsync(int vendorId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new Exception("Mật khẩu mới không được để trống.");

            var user = await _vendorRepo.GetVendorByIdAsync(vendorId);
            if (user == null) throw new Exception("Không tìm thấy tiểu thương này trong hệ thống!");

            var newHash = _passwordHasher.HashPassword(user, newPassword.Trim());
            var updated = await _userRepository.UpdatePasswordAsync(user.UserId, newHash);
            if (!updated)
                throw new Exception("Không thể cập nhật mật khẩu cho tiểu thương.");
        }

        public async Task DeleteVendorAsync(int vendorId)
        {
            var user = await _vendorRepo.GetVendorByIdAsync(vendorId);
            if (user == null) throw new Exception("Không tìm thấy tiểu thương này trong hệ thống!");

            await _vendorRepo.DeleteVendorAsync(user);
        }

        private static string GenerateRandomPassword(int length)
        {
            var result = new char[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = PasswordChars[Random.Shared.Next(PasswordChars.Length)];
            }

            return new string(result);
        }
    }
}
