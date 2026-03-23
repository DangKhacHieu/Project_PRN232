using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByVendorIdAsync(int vendorId)
        {
            // Lọc hóa đơn dựa trên VendorId nằm trong Contract
            return await _context.Invoices
                .Include(i => i.Contract)
                .Where(i => i.Contract != null && i.Contract.VendorId == vendorId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAndVendorIdAsync(int invoiceId, int vendorId)
        {
            // Kiểm tra bảo mật: Hóa đơn này phải thuộc về hợp đồng của đúng Vendor đang đăng nhập
            return await _context.Invoices
                .Include(i => i.Contract)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Contract != null && i.Contract.VendorId == vendorId);
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task<Invoice?> GetInvoiceDetailForExportAsync(int invoiceId, int vendorId)
        {
            return await _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Vendor) // 1. Lấy thông tin VendorProfile
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Stall)  // Lấy thông tin Sạp
                .Include(i => i.InvoiceItems)
                    .ThenInclude(it => it.FeeConfig) // Lấy Cấu hình phí
                    .ThenInclude(it => it.FeeType)   // Lấy Tên loại phí
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Contract != null && i.Contract.VendorId == vendorId);
        }
    }
}