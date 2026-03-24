using BLL.DTOs;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace API_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MomoController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _config;

        public MomoController(IInvoiceService invoiceService, IConfiguration config)
        {
            _invoiceService = invoiceService;
            _config = config;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] MomoWebhookRequestDTO request)
        {
            var momoConfig = _config.GetSection("MomoPaymentConfig").Get<MomoPaymentConfig>();
            if (momoConfig == null) return StatusCode(500, "Momo configuration is missing");

            bool result = await _invoiceService.ProcessMomoWebhookAsync(request, momoConfig);
            
            if (result)
            {
                return NoContent(); // 204 No Content for successful webhook processing
            }

            return BadRequest("Invalid signature or failed to process payout.");
        }
    }
}
