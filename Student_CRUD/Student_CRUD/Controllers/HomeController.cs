using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_CRUD.Models;
using System.Diagnostics;

namespace Student_CRUD.Controllers
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
        public async Task<IActionResult> Index()
        {
            //var stddata = studentDbContext.Students.ToList();
            var stddata = await studentDbContext.Students.ToListAsync();
            
            return View(stddata);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student std)
        {
            if (ModelState.IsValid)
            {
                //studentDbContext.Students.Add(std);
                await studentDbContext.Students.AddAsync(std);
                //studentDbContext.SaveChanges();
                await studentDbContext.SaveChangesAsync();
                TempData["insert_success"] = "Inserted..."; 
                return RedirectToAction("Index", "Home");
            }           
                return View(std);
        }
        public async Task<IActionResult> Details(int? id)
        {
            //var stddata = studentDbContext.Students.ToList();
            if(id == null || studentDbContext.Students == null)
            {
                return NotFound();
            }
            var stddata = await studentDbContext.Students.FirstOrDefaultAsync(x => x.Id == id);

            if(stddata == null)
            {
                return NotFound();
            }
            return View(stddata);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || studentDbContext.Students == null)
            {
                return NotFound();
            }
            var stddata = await studentDbContext.Students.FindAsync(id);
            if (stddata == null)
            {
                return NotFound();
            }
           
            return View(stddata);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Student std)
        {
            if(id != std.Id)
            {
                return NotFound();
            }
            if(ModelState.IsValid)
            {
               studentDbContext.Students.Update(std);
                await studentDbContext.SaveChangesAsync();
                TempData["Update_success"] = "Updated...";
                return RedirectToAction("Index", "Home");
            }
            return View(std);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null || studentDbContext.Students == null)
            {
                return NotFound();
            }
            var stddata = await studentDbContext.Students.FirstOrDefaultAsync(r => r.Id == id);

            if(stddata == null)
            {
                return NotFound();
            }
            return View(stddata);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var stddata = await studentDbContext.Students.FindAsync(id);
            if(stddata != null)
            {
                studentDbContext.Students.Remove(stddata);
            }
            await studentDbContext.SaveChangesAsync();
            TempData["Delete_success"] = "Deleted...";
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
