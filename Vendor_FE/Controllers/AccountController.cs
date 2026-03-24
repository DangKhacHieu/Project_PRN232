using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Vendor_FE.Models;
using System.Net.Http.Headers;

namespace Vendor_FE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;

        public AccountController(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpFactory = httpFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpFactory.CreateClient("BackendAPI");

            try
            {
                var response = await client.PostAsJsonAsync("api/auth/login", new
                {
                    Identifier = model.Identifier,
                    Password = model.Password
                });

                if (!response.IsSuccessStatusCode)
                {
                    model.Error = "Sai tài khoản hoặc mật khẩu";
                    return View(model);
                }

                var data = await response.Content.ReadFromJsonAsync<LoginResultDto>();
                if (data == null)
                {
                    model.Error = "Phản hồi từ server không hợp lệ";
                    return View(model);
                }

                Response.Cookies.Append("VendorAuth", data.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                });

                if (data.RoleId == 1)
                    return Redirect("/Admin");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                model.Error = "Không kết nối được server: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Email = model.Email?.Trim();
            model.Phone = model.Phone?.Trim();
            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.BusinessName = model.BusinessName?.Trim();

            var client = _httpFactory.CreateClient("BackendAPI");

            try
            {
                var response = await client.PostAsJsonAsync("api/auth/register", new
                {
                    FullName = model.FullName,
                    BusinessName = model.BusinessName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password
                });

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = "Đăng ký thất bại";

                    try
                    {
                        var error = await response.Content.ReadFromJsonAsync<ErrorDto>();
                        if (!string.IsNullOrWhiteSpace(error?.message))
                            errorMessage = error.message!;
                    }
                    catch
                    {
                        var raw = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(raw))
                            errorMessage = raw;
                    }

                    model.Error = errorMessage;
                    return View(model);
                }

                TempData["SuccessMessage"] = "Đăng ký thành công, vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                model.Error = "Không kết nối được server: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("VendorAuth");
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult RequestOtpEmail()
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            return View(new RequestOtpEmailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOtpEmail(RequestOtpEmailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var payload = new { Email = model.Email };
                var resp = await client.PostAsJsonAsync("api/auth/request-change-password-otp", payload);
                if (resp.IsSuccessStatusCode)
                {
                    TempData["OtpEmail"] = model.Email;
                    TempData["SuccessMessage"] = "OTP was requested. Check your email.";
                    return RedirectToAction("EnterOtp");
                }

                string errorMessage = "Failed to request OTP.";
                try
                {
                    var media = resp.Content.Headers.ContentType?.MediaType;
                    if (string.Equals(media, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
                        if (!string.IsNullOrWhiteSpace(err?.message))
                            errorMessage = err!.message!;
                        else
                            errorMessage = await resp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var text = await resp.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(text))
                            errorMessage = text;
                    }
                }
                catch
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        errorMessage = text;
                }

                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Cannot contact backend: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult EnterOtp()
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            var model = new EnterOtpViewModel
            {
                Email = TempData["OtpEmail"] as string ?? string.Empty
            };

            if (!string.IsNullOrEmpty(model.Email))
                TempData.Keep("OtpEmail");

            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterOtp(EnterOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token)) return RedirectToAction("Login");

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var payload = new { Otp = model.Otp };
                var resp = await client.PostAsJsonAsync("api/auth/verify-change-password-otp", payload);
                if (resp.IsSuccessStatusCode)
                {
                    TempData["OtpVerified"] = "1";
                    TempData["OtpCode"] = model.Otp;

                    if (!string.IsNullOrEmpty(TempData["OtpEmail"] as string))
                        TempData.Keep("OtpEmail");

                    return RedirectToAction("ChangePassword");
                }

                string errorMessage = "OTP verification failed.";
                try
                {
                    var media = resp.Content.Headers.ContentType?.MediaType;
                    if (string.Equals(media, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
                        if (!string.IsNullOrWhiteSpace(err?.message))
                            errorMessage = err!.message!;
                        else
                            errorMessage = await resp.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var text = await resp.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(text))
                            errorMessage = text;
                    }
                }
                catch
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        errorMessage = text;
                }

                ModelState.AddModelError(string.Empty, errorMessage);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Cannot contact backend: " + ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            if (TempData["OtpVerified"] == null)
                return RedirectToAction("EnterOtp");

            var otpEmail = TempData["OtpEmail"] as string;
            var successMessage = TempData["SuccessMessage"] as string;

            ViewBag.OtpEmail = otpEmail;
            ViewBag.SuccessMessage = successMessage;

            TempData.Keep("OtpVerified");
            TempData.Keep("OtpCode");
            TempData.Keep("OtpEmail");

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            var otpEmail = TempData["OtpEmail"] as string;
            ViewBag.OtpEmail = otpEmail;

            if (!string.IsNullOrEmpty(otpEmail))
                TempData.Keep("OtpEmail");

            if (TempData["OtpVerified"] == null)
                return RedirectToAction("EnterOtp");

            var otp = TempData["OtpCode"] as string;
            if (string.IsNullOrEmpty(otp))
                return RedirectToAction("EnterOtp");

            if (!ModelState.IsValid)
                return View(model);

            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var payload = new
                {
                    Otp = otp,
                    NewPassword = model.NewPassword
                };

                var resp = await client.PostAsJsonAsync("api/auth/change-password-with-otp", payload);

                if (resp.IsSuccessStatusCode)
                {
                    Response.Cookies.Delete("VendorAuth");
                    TempData.Remove("OtpVerified");
                    TempData.Remove("OtpCode");
                    TempData.Remove("OtpEmail");

                    TempData["SuccessMessage"] = "Password changed successfully. Please login again.";
                    return RedirectToAction("Login");
                }

                var err = await resp.Content.ReadFromJsonAsync<ErrorDto>();
                model.Error = err?.message ?? "Failed to change password.";
                return View(model);
            }
            catch (Exception ex)
            {
                model.Error = "Cannot contact backend: " + ex.Message;
                return View(model);
            }
        }

        private class LoginResultDto
        {
            public string Token { get; set; } = null!;
            public int UserId { get; set; }
            public int? VendorId { get; set; }
            public string? FullName { get; set; }
            public string? Role { get; set; }
            public int RoleId { get; set; }
        }

        private class ErrorDto
        {
            public string? message { get; set; }
        }
    }
}