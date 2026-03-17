using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace BLL.Services.Implementations
{
    public class SupportTicketService : ISupportTicketService
    {
        private readonly ISupportTicketRepository _ticketRepo;
        private readonly IWebHostEnvironment _env;

        public SupportTicketService(ISupportTicketRepository ticketRepo, IWebHostEnvironment env)
        {
            _ticketRepo = ticketRepo;
            _env = env;
        }

        public async Task<IEnumerable<TicketResponseDTO>> GetTicketsByVendorIdAsync(int vendorId)
        {
            var tickets = await _ticketRepo.GetByVendorIdAsync(vendorId);

            return tickets.Select(t => new TicketResponseDTO
            {
                TicketId = t.TicketId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                // Trích xuất list chuỗi URL từ List object TicketImage
                ImageUrls = t.TicketImages.Select(img => img.ImageUrl!).ToList()
            });
        }

        public async Task<int> CreateTicketAsync(int vendorId, TicketCreateRequestDTO request)
        {
            // 1. Khởi tạo Entity Ticket
            var ticket = new SupportTicket
            {
                VendorId = vendorId,
                Title = request.Title,
                Description = request.Description,
                Status = "PENDING", // Trạng thái mặc định chờ xử lý
                CreatedAt = DateTime.Now,
                TicketImages = new List<TicketImage>()
            };

            // 2. Xử lý lưu danh sách file ảnh nếu có
            if (request.Images != null && request.Images.Any())
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "tickets");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                foreach (var file in request.Images)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        // Thêm vào collection, EF Core sẽ tự động insert vào bảng phụ TicketImage
                        ticket.TicketImages.Add(new TicketImage
                        {
                            ImageUrl = "/uploads/tickets/" + uniqueFileName
                        });
                    }
                }
            }

            // 3. Lưu vào Database
            await _ticketRepo.CreateAsync(ticket);
            return ticket.TicketId;
        }
    }
}
