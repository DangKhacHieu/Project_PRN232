using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Vendor_FE.Models;

namespace Vendor_FE.Controllers
{
    public class VendorProfilesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VendorProfilesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            AddToken(client);
            var response = await client.GetAsync("api/VendorProfiles/my-profile");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var profile = JsonSerializer.Deserialize<VendorProfileResponseDTO>(content, options);
                return View(profile);
            }

            return View(new VendorProfileResponseDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Index(string BusinessName, string Description, IFormFile? NewCoverImage)
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            AddToken(client);
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(BusinessName ?? ""), "BusinessName");
            content.Add(new StringContent(Description ?? ""), "Description");

            if (NewCoverImage != null)
            {
                var streamContent = new StreamContent(NewCoverImage.OpenReadStream());
                streamContent.Headers.Add("Content-Type", NewCoverImage.ContentType);
                content.Add(streamContent, "Avatar", NewCoverImage.FileName);
            }

            var response = await client.PutAsync("api/VendorProfiles/description", content);
            
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật thất bại!";
            }

            return RedirectToAction(nameof(Index));
        }

        private void AddToken(HttpClient client)
        {
            var token = Request.Cookies["VendorAuth"];
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
