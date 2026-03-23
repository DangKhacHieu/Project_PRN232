using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IPhotoService
    {
        Task<string?> AddPhotoAsync(IFormFile file, string folder = "vendors");
        // string? DeletePhotoAsync(string publicId); // Có thể thêm sau nếu cần
    }
}
