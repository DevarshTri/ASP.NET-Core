using Microsoft.AspNetCore.Mvc;
using Student_CRUD_Wihout_Entity.Data;
using Student_CRUD_Wihout_Entity.Models;
using System.Diagnostics;

namespace Student_CRUD_Wihout_Entity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PersonRepository personrepo;

        public HomeController(ILogger<HomeController> logger , PersonRepository personrepo)
        {
            _logger = logger;
            this.personrepo = personrepo;
        }

        public async Task<IActionResult> Index()
        {
            var students = await personrepo.GetAllAsync();
            return View(students);
        }
        public async Task<IActionResult> Get(int id)
        {
            var students = await personrepo.GetbyIdAsync(id);
            return View(students);
        }
        public async Task<IActionResult> Create(Person person)
        {
            if (ModelState.IsValid)
            {
                await personrepo.AddAsync(person);
                return RedirectToAction("Index");
            }
            return View(person);
        }
        public async Task<IActionResult> Update(Person person , int id)
        {
            if(id != person.Id)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                await personrepo.UpdateAsync(person);
                return RedirectToAction("Index");
            }
            return View(person);
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                await personrepo.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            return View(id);
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
