using Microsoft.AspNetCore.Mvc;
using STSL.SmartLocker.Utils.Enrolment.WebApp.Models;
using STSL.SmartLocker.Utils.Enrolment.WebApp.Services;
using System.Text.RegularExpressions;

namespace STSL.SmartLocker.Utils.Enrolment.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcessSwipe(string captureText)
        {
            const int maxLength = 100;

            string ipAddress = Request.HttpContext.Connection.RemoteIpAddress!.ToString();

            _logger.LogInformation("Processing swipe from IP address {ipAddress}", ipAddress);

            // detect empty input
            if (string.IsNullOrWhiteSpace(captureText))
            {
                _logger.LogInformation("Empty input");
                return Json(new ResponseModel { Success = false, Message = "No input provided" });
            }

            // protect against attack with excessive input payloads
            if (captureText.Length > maxLength)
            {
                _logger.LogInformation("Input string length {length} exceeded {maxLength} characters", captureText.Length, maxLength);
                return Json(new ResponseModel { Success = false, Message = $"Input exceeds {maxLength} characters" });
            }

            Regex rg = new Regex($"^csn:(?<csn>\\d+)\\s+mifarehid:(?<mifarehid>\\d+)$");
            var match = rg.Matches(captureText).SingleOrDefault();

            if (match == null)
            {
                _logger.LogInformation("Invalid input '{input}'", captureText);
                return Json(new ResponseModel { Success = false, Message = "Incorrect format" });
            }

            long csn = long.Parse(match.Groups["csn"].Value);
            long mifareHID = long.Parse(match.Groups["mifarehid"].Value);

            var capture = new CardCredentialCapture { CSN = csn, MifareHID = mifareHID };

            await _emailService.SendEmailAsync(capture, ipAddress);

            return Json(new ResponseModel { Success = true, Message = "Thank you - Badge Details submitted" });
        }

    }
}