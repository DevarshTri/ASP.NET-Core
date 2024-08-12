using Connect_with_pgsql.Data;
using Connect_with_pgsql.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Connect_with_pgsql.Controllers
{
    public class HomeController : Controller
    {
        private readonly StudentDbContext studentDbContext;
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        public HomeController(StudentDbContext studentDbContext)
        {
            this.studentDbContext = studentDbContext;
        }
        public IActionResult Index()
        {
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
