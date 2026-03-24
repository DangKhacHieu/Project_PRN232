using System.Threading.Tasks;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DAL.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailOrPhoneAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier)) return null;

            return await _db.Users
                .Include(u => u.VendorProfile)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    (u.Email != null && u.Email == identifier) ||
                    (u.Phone != null && u.Phone == identifier));
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users
                .Include(u => u.VendorProfile)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<bool> ExistsByEmailOrPhoneAsync(string email, string phone)
        {
            return await _db.Users.AnyAsync(u =>
                (!string.IsNullOrEmpty(email) && u.Email == email) ||
                (!string.IsNullOrEmpty(phone) && u.Phone == phone));
        }

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<VendorProfile> CreateVendorProfileAsync(VendorProfile profile)
        {
            _db.VendorProfiles.Add(profile);
            await _db.SaveChangesAsync();
            return profile;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = newPasswordHash;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ChangePasswordOtp> CreateOtpAsync(ChangePasswordOtp otp)
        {
            _db.ChangePasswordOtps.Add(otp);
            await _db.SaveChangesAsync();
            return otp;
        }

        public async Task<ChangePasswordOtp?> GetLatestOtpAsync(int userId)
        {
            return await _db.ChangePasswordOtps
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkOtpUsedAsync(int otpId)
        {
            var otp = await _db.ChangePasswordOtps.FindAsync(otpId);
            if (otp == null) return false;
            otp.Used = true;
            _db.ChangePasswordOtps.Update(otp);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}