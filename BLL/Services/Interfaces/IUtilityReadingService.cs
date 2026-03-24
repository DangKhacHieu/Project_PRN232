using BLL.DTOs;
using System.IO;
using System.Threading.Tasks;

namespace BLL.Services.Interfaces
{
    public interface IUtilityReadingService
    {
        Task<UtilityReadingResponseDTO?> RecordUtilityAsync(UtilityReadingInputDTO dto);
        Task<int> ImportExcelAsync(Stream fileStream, int month, int year);
        Task<IEnumerable<StallLookupDTO>> GetAllStallsAsync();
    }
}
