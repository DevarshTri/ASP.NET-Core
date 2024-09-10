using DataTransfer_Service.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataTransfer_Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<IDataTransferService> _logger;


        public HomeController(ILogger<IDataTransferService> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                _logger.LogInformation("Data transfer started at {Time}", DateTime.Now);

                // Your data transfer logic here
                _logger.LogInformation("Data transfer in progress...");

                // Simulate success
                _logger.LogInformation("Data transfer completed successfully at {Time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while transferring data.");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
