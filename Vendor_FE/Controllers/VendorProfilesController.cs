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
        public async Task<IActionResult> Index(string StoreName, string Description, IFormFile? NewCoverImage)
        {
            var client = _httpClientFactory.CreateClient("BackendAPI");
            using var content = new MultipartFormDataContent();
            
            content.Add(new StringContent(StoreName ?? ""), "StoreName");
            content.Add(new StringContent(Description ?? ""), "Description");

            if (NewCoverImage != null)
            {
                var streamContent = new StreamContent(NewCoverImage.OpenReadStream());
                streamContent.Headers.Add("Content-Type", NewCoverImage.ContentType);
                content.Add(streamContent, "NewCoverImage", NewCoverImage.FileName);
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
    }
}
