using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vendor_FE.Models;

namespace Vendor_FE.Controllers
{
    public class VendorController : Controller
    {
        private readonly IHttpClientFactory _httpFactory;

        public VendorController(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Profile(int? id)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = null;
            if (!id.HasValue)
            {
                try
                {
                    var parts = token.Split('.');
                    if (parts.Length >= 2)
                    {
                        var payload = parts[1];
                        var json = Base64UrlDecode(payload);
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("vendor_id", out var claim))
                        {
                            if (claim.ValueKind == JsonValueKind.Number && claim.TryGetInt32(out var vid))
                                id = vid;
                            else if (claim.ValueKind == JsonValueKind.String && int.TryParse(claim.GetString(), out var vid2))
                                id = vid2;
                        }

                        if (doc.RootElement.TryGetProperty("sub", out var subClaim))
                        {
                            if (subClaim.ValueKind == JsonValueKind.Number && subClaim.TryGetInt32(out var uid))
                                userId = uid;
                            else if (subClaim.ValueKind == JsonValueKind.String && int.TryParse(subClaim.GetString(), out var uid2))
                                userId = uid2;
                        }
                    }
                }
                catch
                {
                    // ignore parsing errors
                }
            }

            if (!id.HasValue && !userId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Vendor id or User id not provided and not present in token.");
                return View(new VendorProfileViewModel());
            }

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                HttpResponseMessage resp;
                if (id.HasValue && id.Value > 0)
                {
                    resp = await client.GetAsync($"api/vendorprofiles/{id.Value}");
                }
                else
                {
                    resp = await client.GetAsync($"api/vendorprofiles/by-user/{userId.Value}");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Cannot load vendor profile from server.");
                    return View(new VendorProfileViewModel { VendorId = id, UserId = userId });
                }

                var dto = await resp.Content.ReadFromJsonAsync<VendorProfileDto>();
                var model = new VendorProfileViewModel
                {
                    VendorId = dto?.VendorId,
                    UserId = dto?.UserId,
                    FullName = dto?.FullName,
                    BusinessName = dto?.BusinessName,
                    Description = dto?.Description,
                    CoverImageUrl = dto?.CoverImageUrl
                };

                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error contacting backend API.");
                return View(new VendorProfileViewModel { VendorId = id, FullName = User?.Identity?.Name });
            }
        }

        // GET: Vendor/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            int? userId = null;
            if (!id.HasValue)
            {
                try
                {
                    var parts = token.Split('.');
                    if (parts.Length >= 2)
                    {
                        var payload = parts[1];
                        var json = Base64UrlDecode(payload);
                        using var doc = JsonDocument.Parse(json);
                        
                        if (doc.RootElement.TryGetProperty("vendor_id", out var claim))
                        {
                            if (claim.ValueKind == JsonValueKind.Number && claim.TryGetInt32(out var vid))
                                id = vid;
                            else if (claim.ValueKind == JsonValueKind.String && int.TryParse(claim.GetString(), out var vid2))
                                id = vid2;
                        }

                        if (doc.RootElement.TryGetProperty("sub", out var subClaim))
                        {
                            if (subClaim.ValueKind == JsonValueKind.Number && subClaim.TryGetInt32(out var uid))
                                userId = uid;
                            else if (subClaim.ValueKind == JsonValueKind.String && int.TryParse(subClaim.GetString(), out var uid2))
                                userId = uid2;
                        }
                    }
                }
                catch {}
            }

            if (!id.HasValue && !userId.HasValue) return BadRequest();

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                HttpResponseMessage resp;
                if (id.HasValue && id.Value > 0)
                {
                    resp = await client.GetAsync($"api/vendorprofiles/{id.Value}");
                }
                else
                {
                    resp = await client.GetAsync($"api/vendorprofiles/by-user/{userId.Value}");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Cannot load vendor profile from server.");
                    return View(new VendorEditViewModel { VendorId = id, UserId = userId });
                }

                var dto = await resp.Content.ReadFromJsonAsync<VendorProfileDto>();
                var model = new VendorEditViewModel
                {
                    VendorId = dto?.VendorId,
                    UserId = dto?.UserId,
                    FullName = dto?.FullName,
                    BusinessName = dto?.BusinessName,
                    Description = dto?.Description,
                    CoverImageUrl = dto?.CoverImageUrl
                };

                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error contacting backend API.");
                return View(new VendorEditViewModel { VendorId = id });
            }
        }

        // POST: Vendor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VendorEditViewModel model)
        {
            var token = Request.Cookies["VendorAuth"];
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            // add text fields
            if (!string.IsNullOrEmpty(model.BusinessName))
                content.Add(new StringContent(model.BusinessName), "BusinessName");
            if (!string.IsNullOrEmpty(model.Description))
                content.Add(new StringContent(model.Description), "Description");
            if (!string.IsNullOrEmpty(model.FullName))
                content.Add(new StringContent(model.FullName), "FullName");

            // add file if provided
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var stream = model.AvatarFile.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.AvatarFile.ContentType ?? "application/octet-stream");
                // field name expected by backend: "avatar" (adjust if backend expects different)
                content.Add(streamContent, "avatar", model.AvatarFile.FileName);
            }

            try
            {
                // send multipart PUT to backend; backend should accept multipart at this route
                var resp = await client.PutAsync($"api/vendorprofiles/{model.VendorId}", content);
                if (resp.IsSuccessStatusCode)
                {
                    return RedirectToAction("Profile", new { id = model.VendorId });
                }

                // read error message if any
                string err = await resp.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, "Update failed: " + (string.IsNullOrWhiteSpace(err) ? resp.ReasonPhrase : err));
                return View(model);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Error contacting backend API.");
                return View(model);
            }
        }

        // Minimal DTO matching backend response
        private class VendorProfileDto
        {
            public int VendorId { get; set; }
            public int? UserId { get; set; }
            public string? BusinessName { get; set; }
            public string? Description { get; set; }
            public string? CoverImageUrl { get; set; }
            public string? FullName { get; set; }
        }

        private static string Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}