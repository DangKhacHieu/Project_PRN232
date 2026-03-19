using Microsoft.AspNetCore.Mvc;
using Vendor_FE.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Vendor_FE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly HttpClient _client;

        public ProductsController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("BackendAPI");
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("/api/Products");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductResponseDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(products);
            }
            return View(new List<ProductResponseDTO>());
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateRequestDTO request, IFormFile? Image)
        {
            if (!ModelState.IsValid) return View(request);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.ProductName), "ProductName");
            content.Add(new StringContent(request.CategoryId.ToString()), "CategoryId");
            content.Add(new StringContent(request.Unit ?? ""), "Unit");
            content.Add(new StringContent(request.Price.ToString()), "Price");

            if (Image != null)
            {
                var streamContent = new StreamContent(Image.OpenReadStream());
                streamContent.Headers.Add("Content-Type", Image.ContentType);
                content.Add(streamContent, "Image", Image.FileName);
            }

            var response = await _client.PostAsync("/api/Products", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            
            ModelState.AddModelError("", "Thêm sản phẩm thất bại.");
            return View(request);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync("/api/Products");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<ProductResponseDTO>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var product = products?.FirstOrDefault(p => p.ProductId == id);
                if (product != null)
                {
                    var updateDto = new ProductUpdateRequestDTO
                    {
                        ProductName = product.ProductName,
                        CategoryId = product.CategoryId,
                        Unit = product.Unit,
                        Price = product.Price,
                        IsActive = product.IsActive
                    };
                    ViewBag.ProductId = id;
                    ViewBag.ImageUrl = product.ImageUrl;
                    return View(updateDto);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductUpdateRequestDTO request, IFormFile? NewImage)
        {
            if (!ModelState.IsValid) return View(request);

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.ProductName), "ProductName");
            if (request.CategoryId.HasValue) content.Add(new StringContent(request.CategoryId.ToString()), "CategoryId");
            if (request.Unit != null) content.Add(new StringContent(request.Unit), "Unit");
            content.Add(new StringContent(request.Price.ToString()), "Price");
            content.Add(new StringContent(request.IsActive.ToString()), "IsActive");

            if (NewImage != null)
            {
                var streamContent = new StreamContent(NewImage.OpenReadStream());
                streamContent.Headers.Add("Content-Type", NewImage.ContentType);
                content.Add(streamContent, "NewImage", NewImage.FileName);
            }

            var response = await _client.PutAsync($"/api/Products/{id}", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Cập nhật thất bại.");
            ViewBag.ProductId = id;
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.DeleteAsync($"/api/Products/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
