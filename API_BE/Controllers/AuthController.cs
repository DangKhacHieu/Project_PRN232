using BLL.Services.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BLL.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Invalid payload." });

            var result = await _auth.AuthenticateAsync(request);
            if (result == null) return Unauthorized(new { message = "Invalid credentials." });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid payload." });

            var result = await _auth.RegisterAsync(request);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            // return created (could return location to resource)
            return Created("", result);
        }

        // POST: api/auth/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Invalid payload." });
            }

            // read user id from token: try 'sub' then fallback to NameIdentifier
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(sub, out var userId))
                return Unauthorized(new { message = "Invalid token." });

            var changed = await _auth.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (!changed)
                return BadRequest(new { message = "Current password is incorrect or change failed." });

            return Ok(new { message = "Password changed." });
        }

        public class ChangePasswordRequest
        {
            [Required]
            public string CurrentPassword { get; set; } = null!;
            [Required]
            public string NewPassword { get; set; } = null!;
        }

        public class RequestOtpDto
        {
            [Required]
            public string Email { get; set; } = null!;
        }

        // POST: api/auth/request-change-password-otp
        [HttpPost("request-change-password-otp")]
        [Authorize]
        public async Task<IActionResult> RequestChangePasswordOtp([FromBody] RequestOtpDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required." });

            // read user id from token: try 'sub' then fallback to NameIdentifier
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(sub, out var userId))
                return Unauthorized(new { message = "Invalid token." });

            var sent = await _auth.SendChangePasswordOtpAsync(userId, request.Email);
            if (!sent) return BadRequest(new { message = "Cannot send OTP. Email may not match your account or server SMTP not configured." });
            return Ok(new { message = "OTP sent." });
        }

        // POST: api/auth/change-password-with-otp
        [HttpPost("change-password-with-otp")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordWithOtp([FromBody] ChangePasswordWithOtpRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Invalid payload." });

            // read user id from token
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(sub, out var userId))
                return Unauthorized(new { message = "Invalid token." });

            var changed = await _auth.ChangePasswordWithOtpAsync(userId, request.Otp, request.NewPassword);
            if (!changed)
                return BadRequest(new { message = "OTP invalid/expired or change failed." });

            return Ok(new { message = "Password changed with OTP." });
        }

        public class ChangePasswordWithOtpRequest
        {
            [Required]
            public string Otp { get; set; } = null!;
            [Required]
            public string NewPassword { get; set; } = null!;
        }

        public class VerifyOtpDto
        {
            [Required]
            public string Otp { get; set; } = null!;
        }

        // POST: api/auth/verify-change-password-otp
        [HttpPost("verify-change-password-otp")]
        [Authorize]
        public async Task<IActionResult> VerifyChangePasswordOtp([FromBody] VerifyOtpDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Otp))
                return BadRequest(new { message = "OTP is required." });

            var sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(sub, out var userId))
                return Unauthorized(new { message = "Invalid token." });

            var ok = await _auth.VerifyChangePasswordOtpAsync(userId, request.Otp);
            if (!ok) return BadRequest(new { message = "OTP invalid or expired." });

            return Ok(new { message = "OTP verified." });
        }
    }
}