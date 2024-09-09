using DataTransfer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DataTransfer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataTransferService dataTransferService;

        public HomeController(ILogger<HomeController> logger , IDataTransferService dataTransferService)
        {
            _logger = logger;
            this.dataTransferService = dataTransferService;
        }

        public async Task<IActionResult> Index()
        {
            await dataTransferService.TransferDataAsync();
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
