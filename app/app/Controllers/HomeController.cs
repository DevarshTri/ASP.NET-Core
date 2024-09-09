using app.Data;
using app.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmployeeRepo employeeRepo;

        public HomeController(ILogger<HomeController> logger , EmployeeRepo employeeRepo)
        {
            _logger = logger;
            this.employeeRepo = employeeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var emp_data = await employeeRepo.GetEmployees();
            return View(emp_data);
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
