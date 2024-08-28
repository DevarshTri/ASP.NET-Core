using Expense_App.Data;
using Expense_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Expense_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ExpenseRepo expenseRepo;

        public HomeController(ILogger<HomeController> logger , ExpenseRepo expenseRepo)
        {
            _logger = logger;
            this.expenseRepo = expenseRepo;
        }

        public async  Task<IActionResult> Index()
        {
            var expense_data = await expenseRepo.GetExpensesAsync();
            return View(expense_data);
        }
        public async Task<IActionResult> Create(Expense expense)
        {
                await expenseRepo.AddAsync(expense);
                return RedirectToAction("Index");
            return View(expense);
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
