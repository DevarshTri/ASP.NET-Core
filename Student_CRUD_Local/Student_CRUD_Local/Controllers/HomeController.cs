using Microsoft.AspNetCore.Mvc;
using Student_CRUD_Local.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Student_CRUD_Local.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public StudentService StudentService { get; }

        public HomeController(ILogger<HomeController> logger , StudentService studentService)
        {
            _logger = logger;
            StudentService = studentService;
        }

        public IActionResult Index()
        {
            var data = StudentService.GetStudents();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Student std)
        {
            if(ModelState.IsValid)
            {
                StudentService.Students.Add(std);
                Console.WriteLine(std);
                return RedirectToAction("Index");
            }
            return View(std);
        }
        //public IActionResult Details()
        //{
        //    return View();
        //}
        public IActionResult Details(int? id)
        {
            if(id == null && StudentService.Students.Count == 0)
            {
                return NotFound();
            }
            var data = StudentService.Students.FirstOrDefault(x => x.Id == id);
            if(data == null)
            {
                return NotFound();
            }
            return View(data);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null && StudentService.Students.Count == 0)
            {
                return NotFound();
            }
            var data = StudentService.Students.FirstOrDefault(x => x.Id == id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }
        [HttpPost]
        public IActionResult Edit(Student std , int? id )
        {
            if(id != std.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                var exstd = StudentService.Students.FirstOrDefault(x => x.Id == id);
                exstd.Name = std.Name;
                exstd.phone = std.phone;
                return RedirectToAction("Index");
            }
            return View(std);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null && StudentService.Students.Count == 0)
            {
                return NotFound();
            }
            var data = StudentService.Students.FirstOrDefault(x => x.Id == id);
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int? id)
        {
            var stddata = StudentService.Students.FirstOrDefault(x => x.Id == id);
            
            if (stddata != null)
            {
                StudentService.Students.Remove(stddata);
                return RedirectToAction("Index");
            }
            return View(stddata);
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
