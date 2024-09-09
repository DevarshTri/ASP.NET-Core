using Employee_App.Data;
using Employee_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Employee_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmployeeService employeeService;

        public HomeController(ILogger<HomeController> logger, EmployeeService employeeService)
        {
            _logger = logger;
            this.employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var emp_data = await employeeService.GetEmployeesAsync();
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
