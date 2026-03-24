using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BLL.Models;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IDistributedCache _cache;

        public AuthService(IUserRepository userRepository, IConfiguration config, IDistributedCache cache)
        {
            _userRepository = userRepository;
            _config = config;
            _passwordHasher = new PasswordHasher<User>();
            _cache = cache;
        }

        public async Task<LoginResult?> AuthenticateAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailOrPhoneAsync(request.Identifier);
            if (user == null || !user.IsActive) return null;

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Failed) return null;

            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
            var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim("role", user.Role?.RoleName ?? string.Empty),
                new Claim("role_id", user.RoleId.ToString())
            };

            if (user.VendorProfile != null)
                claims.Add(new Claim("vendor_id", user.VendorProfile.VendorId.ToString()));

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResult
            {
                Token = tokenString,
                UserId = user.UserId,
                VendorId = user.VendorProfile?.VendorId,
                FullName = user.FullName,
                Role = user.Role?.RoleName,
                RoleId = user.RoleId 
            };
        }

        public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone)))
            {
                return new RegisterResult { Success = false, Message = "FullName, password and either email or phone are required." };
            }

            // check existence
            var exists = await _userRepository.ExistsByEmailOrPhoneAsync(request.Email ?? string.Empty, request.Phone ?? string.Empty);
            if (exists)
            {
                return new RegisterResult { Success = false, Message = "Email or phone already registered." };
            }

            // determine RoleId for vendor
            // You can override via config: DefaultRoles:VendorId
            var roleId = _config.GetValue<int?>("DefaultRoles:VendorId") ?? 2;

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                RoleId = roleId,
                IsActive = true
            };

            // hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // create user
            try
            {
                var createdUser = await _userRepository.CreateAsync(user);

                int? vendorId = null;
                if (!string.IsNullOrWhiteSpace(request.BusinessName))
                {
                    var profile = new VendorProfile
                    {
                        UserId = createdUser.UserId,
                        BusinessName = request.BusinessName
                    };
                    var createdProfile = await _userRepository.CreateVendorProfileAsync(profile);
                    vendorId = createdProfile.VendorId;
                }

                return new RegisterResult
                {
                    Success = true,
                    Message = "Registration successful.",
                    UserId = createdUser.UserId,
                    VendorId = vendorId
                };
            }
            catch (Exception ex)
            {
                // include inner exception message for diagnostics (safe for dev)
                var inner = ex.InnerException?.Message;
                var msg = "Registration failed: " + ex.Message + (string.IsNullOrWhiteSpace(inner) ? "" : " | Inner: " + inner);
                return new RegisterResult { Success = false, Message = msg };
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
            if (verify == PasswordVerificationResult.Failed) return false;

            var newHash = _passwordHasher.HashPassword(user, newPassword);
            return await _userRepository.UpdatePasswordAsync(userId, newHash);
        }

        public async Task<bool> SendChangePasswordOtpAsync(int userId, string email)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(email)) return false;
            if (!string.Equals(user.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
                return false;

            var rng = new Random();
            var code = rng.Next(100000, 999999).ToString();

            // store in distributed cache for 10 minutes
            var key = $"otp:{userId}";
            var otpData = new { Code = code, ExpiresAt = DateTime.UtcNow.AddMinutes(10) };
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(otpData), options);

            // send email via SMTP (keep existing SMTP code)
            var host = _config["Smtp:Host"];
            if (string.IsNullOrEmpty(host)) return false;
            var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
            var from = _config["Smtp:From"] ?? user.Email;
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];
            var enableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) ? ssl : true;

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = string.IsNullOrEmpty(username) ? null : new NetworkCredential(username, password)
                };
                var mail = new MailMessage(from, user.Email)
                {
                    Subject = "OTP for password change",
                    Body = $"Your OTP code is: {code}. It will expire in 10 minutes.",
                    IsBodyHtml = false
                };
                await client.SendMailAsync(mail);
                return true;
            }
            catch
            {
                // do not persist OTP on send failure
                await _cache.RemoveAsync(key);
                return false;
            }
        }

        public async Task<bool> ChangePasswordWithOtpAsync(int userId, string otp, string newPassword)
        {
            var key = $"otp:{userId}";
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json)) return false;
            var stored = JsonSerializer.Deserialize<CacheOtp>(json);
            if (stored == null || stored.ExpiresAt < DateTime.UtcNow) return false;
            if (!string.Equals(stored.Code, otp, StringComparison.Ordinal)) return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;
            var newHash = _passwordHasher.HashPassword(user, newPassword);
            var updated = await _userRepository.UpdatePasswordAsync(userId, newHash);
            if (!updated) return false;

            await _cache.RemoveAsync(key);
            return true;
        }

        public async Task<bool> VerifyChangePasswordOtpAsync(int userId, string otp)
        {
            if (string.IsNullOrWhiteSpace(otp)) return false;
            var key = $"otp:{userId}";
            var json = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(json)) return false;
            var stored = JsonSerializer.Deserialize<CacheOtp>(json);
            if (stored == null) return false;
            if (stored.ExpiresAt < DateTime.UtcNow) return false;
            return string.Equals(stored.Code, otp, StringComparison.Ordinal);
        }

        private class CacheOtp { public string Code { get; set; } = null!; public DateTime ExpiresAt { get; set; } }
    }
}