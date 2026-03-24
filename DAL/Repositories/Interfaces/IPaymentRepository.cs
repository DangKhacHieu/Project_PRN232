using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByTransactionCodeAsync(string transactionCode);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
    }
}
