using Microsoft.AspNetCore.Mvc;
using Practice1.Data;
using Practice1.Models;
using System.Diagnostics;

namespace Practice1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PersonRepo personRepo;

        public HomeController(ILogger<HomeController> logger , PersonRepo personRepo)
        {
            _logger = logger;
            this.personRepo = personRepo;
        }

        public async Task<IActionResult> Index()
        {
            var persondata = await personRepo.GetPeopleAsync();
            return View(persondata);
        }
        public async Task<IActionResult> Details(int id)
        {
            var persondata = await personRepo.GetId(id);
            return View(persondata);
        }
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Person person)
        {
            if(ModelState.IsValid)
            {
                await personRepo.AddPerson(person);
                return RedirectToAction("Index");
            }
           
            return View(person);
        }
        public async Task<IActionResult> Delete(int id)
        {

            await personRepo.DeletePerson(id);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int id)
        {

            if(id == 0)
            {
                return NotFound();
            }
            var person_data = await personRepo.GetId(id);
            return View(person_data);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Person person, int id)
        {
           if(ModelState.IsValid)
            {
                await personRepo.UpdatePerson(person);
                return RedirectToAction("Index");
            }
           return View(person);
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
