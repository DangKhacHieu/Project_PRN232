using BLL.DTOs;
using BLL.Services.Interfaces;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iText.Layout.Properties; // Cần thiết cho SetBold, TextAlignment

namespace BLL.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IStallContractRepository _contractRepo;
        private readonly IUtilityReadingRepository _utilityRepo;
        private readonly IFeeConfigRepository _feeConfigRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly AppDbContext _context;

        public InvoiceService(
            IInvoiceRepository invoiceRepo,
            IStallContractRepository contractRepo,
            IUtilityReadingRepository utilityRepo,
            IFeeConfigRepository feeConfigRepo,
            IPaymentRepository paymentRepo,
            AppDbContext context)
        {
            _invoiceRepo = invoiceRepo;
            _contractRepo = contractRepo;
            _utilityRepo = utilityRepo;
            _feeConfigRepo = feeConfigRepo;
            _paymentRepo = paymentRepo;
            _context = context;
        }

        public async Task<IEnumerable<InvoiceResponseDTO>> GetInvoicesByVendorIdAsync(int vendorId)
        {
            var invoices = await _invoiceRepo.GetInvoicesByVendorIdAsync(vendorId);
            return invoices.Select(i => MapToResponseDTO(i));
        }

        public async Task<IEnumerable<InvoiceResponseDTO>> GetAllInvoicesAsync()
        {
            var invoices = await _invoiceRepo.GetAllInvoicesAsync();
            return invoices.Select(i => MapToResponseDTO(i));
        }

        public async Task<InvoiceDetailExportDTO?> GetInvoiceDetailAsync(int vendorId, int invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceDetailForExportAsync(invoiceId, vendorId);
            if (invoice == null) return null;

            return new InvoiceDetailExportDTO
            {
                InvoiceId = invoice.InvoiceId,
                BusinessName = invoice.Contract?.Vendor?.BusinessName ?? "Khách hàng",
                StallCode = invoice.Contract?.Stall?.StallCode ?? "Chưa cập nhật",
                Month = invoice.Month,
                Year = invoice.Year,
                TotalAmount = invoice.TotalAmount ?? 0,
                Status = invoice.Status,
                Items = invoice.InvoiceItems.Select(item => new InvoiceItemExportDTO
                {
                    FeeName = item.FeeConfig?.FeeType?.Name ?? "Phí dịch vụ",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount ?? 0
                }).ToList()
            };
        }

        public async Task<IEnumerable<ActiveContractDTO>> GetActiveContractsAsync()
        {
            var contracts = await _contractRepo.GetAllActiveContractsAsync();
            return contracts.Select(c => new ActiveContractDTO
            {
                ContractId = c.ContractId,
                StallId = c.StallId,
                StallCode = c.Stall?.StallCode ?? "",
                BusinessName = c.Vendor?.BusinessName ?? "",
                MonthlyRent = c.MonthlyRent
            });
        }

        public async Task<InvoiceResponseDTO?> CalculateAndGenerateInvoiceAsync(CalculateInvoiceRequestDTO dto)
        {
            var contract = await _contractRepo.GetByIdAsync(dto.ContractId);
            if (contract == null || contract.Status != "ACTIVE")
                throw new Exception("Hợp đồng không khả dụng.");

            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i =>
                i.ContractId == dto.ContractId && i.Month == dto.Month && i.Year == dto.Year);

            if (existingInvoice != null)
                throw new Exception("Hóa đơn tháng này đã tồn tại.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var calculationDate = DateTime.Now;
                var newInvoice = new Invoice
                {
                    ContractId = dto.ContractId,
                    Month = dto.Month,
                    Year = dto.Year,
                    Status = "UNPAID",
                    CreatedAt = calculationDate
                };

                _context.Invoices.Add(newInvoice);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;

                // 1. TÍNH PHÍ DỰA TRÊN CHỈ SỐ (ELEC, WATER)
                var reading = await _utilityRepo.GetByStallAndMonthAsync(contract.StallId, dto.Month, dto.Year);
                var utilityCodes = new[] { "ELEC", "WATER" };

                foreach (var code in utilityCodes)
                {
                    decimal consumption = 0; // Đổi sang decimal để tránh lỗi cast
                    if (reading != null)
                    {
                        consumption = (code == "ELEC")
                            ? (decimal)((reading.ElectricityNew ?? 0) - (reading.ElectricityOld ?? 0))
                            : (decimal)((reading.WaterNew ?? 0) - (reading.WaterOld ?? 0));
                    }

                    if (consumption > 0)
                    {
                        var feeType = await _context.FeeTypes.FirstOrDefaultAsync(f => f.Code == code);
                        if (feeType != null)
                        {
                            var config = await _feeConfigRepo.GetFeeConfigAsync(calculationDate, feeType.FeeTypeId);
                            if (config != null)
                            {
                                var amount = consumption * config.UnitPrice;
                                _context.InvoiceItems.Add(new InvoiceItem
                                {
                                    InvoiceId = newInvoice.InvoiceId,
                                    FeeId = config.FeeId,
                                    Quantity = consumption, // Đã đồng bộ kiểu decimal
                                    UnitPrice = config.UnitPrice,
                                    Amount = amount
                                });
                                totalAmount += amount;
                            }
                        }
                    }
                }

                // 2. TÍNH PHÍ CỐ ĐỊNH (RENT, CLEAN, MARKET)
                var fixedFeeCodes = new[] { "RENT", "CLEAN", "MARKET" };
                foreach (var code in fixedFeeCodes)
                {
                    var feeType = await _context.FeeTypes.FirstOrDefaultAsync(f => f.Code == code);
                    if (feeType != null)
                    {
                        var config = await _feeConfigRepo.GetFeeConfigAsync(calculationDate, feeType.FeeTypeId);

                        decimal unitPrice = (code == "RENT") ? contract.MonthlyRent : (config?.UnitPrice ?? 0);
                        int feeId = config?.FeeId ?? feeType.FeeTypeId;

                        if (unitPrice > 0)
                        {
                            _context.InvoiceItems.Add(new InvoiceItem
                            {
                                InvoiceId = newInvoice.InvoiceId,
                                FeeId = feeId,
                                Quantity = 1,
                                UnitPrice = unitPrice,
                                Amount = unitPrice
                            });
                            totalAmount += unitPrice;
                        }
                    }
                }

                newInvoice.TotalAmount = totalAmount;
                _context.Invoices.Update(newInvoice);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return MapToResponseDTO(newInvoice);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ClearDebtAsync(int invoiceId, PaymentConfirmationDTO payment)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice == null || invoice.Status == "PAID" || invoice.Status == "PENDING") return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newPayment = new Payment
                {
                    InvoiceId = invoice.InvoiceId,
                    PaymentMethod = payment.PaymentMethod,
                    TransactionCode = payment.TransactionCode ?? payment.MomoTransactionId,
                    PaidAmount = payment.PaidAmount,
                    Status = "SUCCESS",
                    PaidAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };

                await _paymentRepo.AddAsync(newPayment);
                // Admin xác nhận gạch nợ thành công thì trạng thái là PAID
                invoice.Status = "PAID";
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception($"ClearDebt error: {msg}");
            }
        }

        public async Task<string?> GenerateVietQRUrlAsync(int vendorId, int invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceByIdAndVendorIdAsync(invoiceId, vendorId);
            if (invoice == null || invoice.Status == "PAID") return null;

            return $"https://img.vietqr.io/image/970436-0123456789-compact.png?amount={invoice.TotalAmount}&addInfo=THANHTOAN_HD_{invoiceId}&accountName=BAN QUAN LY CHO";
        }

        public async Task<byte[]?> GenerateInvoicePdfAsync(int vendorId, int invoiceId)
        {
            var detail = await GetInvoiceDetailAsync(vendorId, invoiceId);
            if (detail == null) return null;

            using var memoryStream = new MemoryStream();
            using var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            using var document = new iText.Layout.Document(pdf);

            document.Add(new iText.Layout.Element.Paragraph("BIEN LAI DIEN TU")
                .SetTextAlignment(TextAlignment.CENTER).SetFontSize(20));
            document.Add(new iText.Layout.Element.Paragraph($"Ma HD: #{detail.InvoiceId} | Khach: {detail.BusinessName} | Sap: {detail.StallCode}"));
            document.Add(new iText.Layout.Element.Paragraph($"Ky thanh toan: {detail.Month}/{detail.Year}"));

            var table = new iText.Layout.Element.Table(4, true);
            table.AddHeaderCell("Loai phi"); table.AddHeaderCell("SL"); table.AddHeaderCell("Don gia"); table.AddHeaderCell("Thanh tien");

            foreach (var item in detail.Items)
            {
                table.AddCell(item.FeeName ?? "");
                table.AddCell(item.Quantity?.ToString() ?? "1");
                table.AddCell(item.UnitPrice?.ToString("N0") ?? "0");
                table.AddCell(item.Amount.ToString("N0"));
            }
            document.Add(table);

            // Sửa lỗi SetBold ở đây
            document.Add(new iText.Layout.Element.Paragraph($"\nTONG CONG: {detail.TotalAmount:N0} VND")
                .SetFontSize(16));

            document.Close();
            return memoryStream.ToArray();
        }

        public async Task<string?> GenerateMomoPaymentUrlAsync(int invoiceId, decimal amount, MomoPaymentConfig config)
        {
            long amountLong = (long)amount;
            string orderId = $"HD{invoiceId}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            string requestId = Guid.NewGuid().ToString();
            string extraData = invoiceId.ToString();
            string orderInfo = $"Thanh toan hoa don {invoiceId}";
            string requestType = "captureWallet";

            // Signature: các field PHẢI theo đúng thứ tự alphabetical và KHỚP với request body
            // Format of signature: accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}
            string rawSignature = $"accessKey={config.AccessKey}&amount={amountLong}&extraData={extraData}&ipnUrl={config.CallbackUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={config.PartnerCode}&redirectUrl={config.ReturnUrl}&requestId={requestId}&requestType={requestType}";
            string signature = ComputeHmacSha256(rawSignature, config.SecretKey);

            var requestData = new
            {
                partnerCode = config.PartnerCode,
                partnerName = "Smart Market",
                storeId = config.PartnerCode,
                requestId,
                amount = amountLong,
                orderId,
                orderInfo,
                redirectUrl = config.ReturnUrl,
                ipnUrl = config.CallbackUrl,
                lang = "vi",
                requestType,
                autoCapture = true,
                extraData,
                signature
            };

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var response = await client.PostAsJsonAsync(config.MomoApiUrl, requestData);
                var resBody = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();

                var resultCode = resBody?["resultCode"]?.GetValue<int>();
                if (resultCode == 0)
                {
                    return resBody?["payUrl"]?.ToString();
                }

                // MoMo từ chối - ném exception với thông tin chi tiết để controller có thể trả về client
                var momoMessage = resBody?["message"]?.ToString() ?? "No message";
                var momoLocalMsg = resBody?["localMessage"]?.ToString() ?? "";
                // Dùng InvalidOperationException để sau đó re-throw khỏi network exception catch
                throw new InvalidOperationException($"MoMo lỗi (resultCode={resultCode}): {momoMessage}. {momoLocalMsg}");
            }
            catch (InvalidOperationException)
            {
                // Re-throw MoMo business errors để controller xử lý
                throw;
            }
            catch (Exception ex)
            {
                // Chỉ nuốt lỗi network/IO
                System.Console.WriteLine($"[MoMo Network Exception] {ex.Message}");
                throw new InvalidOperationException($"Không kết nối được MoMo API: {ex.Message}");
            }
        }


        public async Task<bool> ProcessMomoWebhookAsync(MomoWebhookRequestDTO request, MomoPaymentConfig config)
        {
            string rawSignature = $"accessKey={config.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}&message={request.Message}&orderId={request.OrderId}&orderInfo={request.OrderInfo}&orderType={request.OrderType}&partnerCode={request.PartnerCode}&payType={request.PayType}&requestId={request.RequestId}&responseTime={request.ResponseTime}&resultCode={request.ResultCode}&transId={request.TransId}";
            if (ComputeHmacSha256(rawSignature, config.SecretKey) != request.Signature) return false;

            if (request.ResultCode == 0 && int.TryParse(request.ExtraData, out int invoiceId))
            {
                return await ClearDebtAsync(invoiceId, new PaymentConfirmationDTO
                {
                    PaymentMethod = "MOMO",
                    PaidAmount = request.Amount,
                    TransactionCode = request.OrderId,
                    MomoTransactionId = request.TransId.ToString()
                });
            }
            return false;
        }

        private InvoiceResponseDTO MapToResponseDTO(Invoice i) => new InvoiceResponseDTO
        {
            InvoiceId = i.InvoiceId,
            ContractId = i.ContractId,
            Month = i.Month,
            Year = i.Year,
            TotalAmount = i.TotalAmount ?? 0,
            Status = i.Status,
            CreatedAt = i.CreatedAt
        };

        private string ComputeHmacSha256(string message, string secretKey)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
            using var hmacsha256 = new HMACSHA256(keyByte);
            byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
        }
    }
}