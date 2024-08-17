using Microsoft.AspNetCore.Mvc;
using Student_CRUD_DBF.Models;
using System.Diagnostics;

namespace Student_CRUD_DBF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly Sata1002Context Context;

        public HomeController(ILogger<HomeController> logger, Sata1002Context context)
        {
            _logger = logger;
            Context = context;
        }

        public IActionResult Index()
        {
            var data = Context.Students.ToList();
            return View(data);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Student std)
        {
            Context.Students.Add(std);
            Context.SaveChanges();
            return RedirectToAction("Index","Home");
        }
        public IActionResult Delete(int? id)
        {
            var data = Context.Students.FirstOrDefault(x => x.Id == id);
            return View(data);
        }
        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteConfirm(int? id)
        {
            var data = Context.Students.Find(id);
            if(data != null)
            {
                Context.Students.Remove(data);
            }
            Context.SaveChanges();
            return RedirectToAction("Index", "Home");
            

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
