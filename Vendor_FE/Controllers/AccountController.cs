using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

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
                    model.Error = "Login failed. Check credentials.";
                    return View(model);
                }

                var data = await response.Content.ReadFromJsonAsync<LoginResultDto>();
                if (data == null)
                {
                    model.Error = "Unexpected response from server.";
                    return View(model);
                }

                Response.Cookies.Append("VendorAuth", data.Token, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax
                });

                var roleId = data.RoleId;

                if (roleId == 1)
                {
                    var adminUrl = _config["AdminAppUrl"];
                    if (!string.IsNullOrEmpty(adminUrl))
                        return Redirect(adminUrl);

                    return Redirect("/Admin");
                }

                if (roleId == 2)
                {
                    if (data.VendorId.HasValue)
                        return RedirectToAction("Profile", "Vendor", new { id = data.VendorId.Value });

                    return RedirectToAction("Profile", "Vendor");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                model.Error = "Cannot contact backend: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = _httpFactory.CreateClient("BackendAPI");

            try
            {
                var response = await client.PostAsJsonAsync("api/auth/register", new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    BusinessName = model.BusinessName
                });

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }

                // Robustly read error content: try JSON then fallback to plain text
                string errorMessage = "Registration failed.";
                try
                {
                    var media = response.Content.Headers.ContentType?.MediaType;
                    if (string.Equals(media, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                var err = await response.Content.ReadFromJsonAsync<ErrorDto>();
                model.Error = err?.message ?? "Registration failed.";
                        if (!string.IsNullOrWhiteSpace(err?.message))
                            errorMessage = err.message!;
                        else
                            errorMessage = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        var text = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(text))
                            errorMessage = text;
                    }
                }
                catch
                {
                    var text = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(text))
                        errorMessage = text;
                }

                model.Error = errorMessage;
                return View(model);
            }
            catch (Exception ex)
            {
                model.Error = "Cannot contact backend: " + ex.Message;
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

        // GET: /Account/RequestOtpEmail
        [HttpGet]
        public IActionResult RequestOtpEmail()
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            return View(new RequestOtpEmailViewModel());
        }

        // POST: /Account/RequestOtpEmail
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
                    TempData["OtpEmail"] = model.Email; // lưu email để hiển thị
                    TempData["SuccessMessage"] = "OTP was requested. Check your email.";
                    return RedirectToAction("EnterOtp");
                }

                // existing robust error reading...
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

        // GET EnterOtp
        [HttpGet]
        public IActionResult EnterOtp()
        {
            if (Request.Cookies["VendorAuth"] == null) return RedirectToAction("Login");

            var model = new EnterOtpViewModel
            {
                Email = TempData["OtpEmail"] as string ?? string.Empty
            };

            // keep email for next steps
            if (!string.IsNullOrEmpty(model.Email))
                TempData.Keep("OtpEmail");

            ViewBag.SuccessMessage = TempData["SuccessMessage"] as string;
            return View(model);
        }

        // POST EnterOtp -> call verify endpoint
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
                    // mark verified and store otp temporarily for the change step
                    TempData["OtpVerified"] = "1";
                    TempData["OtpCode"] = model.Otp;
                    // keep email for next page
                    if (!string.IsNullOrEmpty(TempData["OtpEmail"] as string))
                        TempData.Keep("OtpEmail");

                    return RedirectToAction("ChangePassword");
                }

                // read error
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

        // ChangePassword GET: chỉ cho phép vào khi OTP đã verified
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

            // keep data for POST
            TempData.Keep("OtpVerified");
            TempData.Keep("OtpCode");
            TempData.Keep("OtpEmail");

            return View(new ChangePasswordViewModel());
        }

        // ChangePassword POST: send otp + new password to backend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (Request.Cookies["VendorAuth"] == null)
                return RedirectToAction("Login");

            var otpEmail = TempData["OtpEmail"] as string;
            ViewBag.OtpEmail = otpEmail;

            // keep for redisplay if needed
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
                    // clear state
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