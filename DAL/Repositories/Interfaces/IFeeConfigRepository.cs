using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Repositories.Interfaces
{
    public interface IFeeConfigRepository
    {
        Task<FeeConfig?> GetFeeConfigAsync(DateTime date, int feeTypeId);
        Task<IEnumerable<FeeConfig>> GetAllActiveFeeConfigsAsync(DateTime date);
        // FeeTypeId 1: Điện, 2: Nước (tuỳ cấu hình ban đầu)
    }
}
