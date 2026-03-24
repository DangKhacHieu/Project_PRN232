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
        private readonly IPhotoService _photoService;

        public SupportTicketService(ISupportTicketRepository ticketRepo, IPhotoService photoService)
        {
            _ticketRepo = ticketRepo;
            _photoService = photoService;
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
                Status = TicketStatus.Pending,
                CreatedAt = DateTime.Now,
                TicketImages = new List<TicketImage>()
            };

            // 2. Xử lý lưu danh sách file ảnh nếu có
            if (request.Images != null && request.Images.Any())
            {
                foreach (var file in request.Images)
                {
                    if (file.Length > 0)
                    {
                        var uploadedUrl = await _photoService.AddPhotoAsync(file, "support_tickets");
                        if (!string.IsNullOrEmpty(uploadedUrl))
                        {
                            // Thêm vào collection, EF Core sẽ tự động insert vào bảng phụ TicketImage
                            ticket.TicketImages.Add(new TicketImage
                            {
                                ImageUrl = uploadedUrl
                            });
                        }
                    }
                }
            }
            // 3. Lưu vào Database
            await _ticketRepo.CreateAsync(ticket);
            return ticket.TicketId;
        }




        public async Task<bool> ProcessTicketAsync(int ticketId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            if (ticket == null || ticket.Status != TicketStatus.Pending) return false;

            ticket.Status = TicketStatus.Processing;
            await _ticketRepo.UpdateAsync(ticket);
            return true;
        }

        public async Task<bool> ResolveTicketAsync(int ticketId)
        {
            var ticket = await _ticketRepo.GetByIdAsync(ticketId);
            // Có thể bỏ qua status check nếu admin được linh động, nhưng đúng luồng thì từ Processing -> Resolved
            if (ticket == null || (ticket.Status != TicketStatus.Processing && ticket.Status != TicketStatus.Pending)) return false;

            ticket.Status = TicketStatus.Resolved;
            await _ticketRepo.UpdateAsync(ticket);
            return true;
        }
    }
}
