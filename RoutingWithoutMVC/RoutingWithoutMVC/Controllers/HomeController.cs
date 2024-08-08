using Microsoft.AspNetCore.Mvc;

namespace RoutingWithoutMVC.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        [Route("")]
        //[Route("Home")]
        //[Route("[action]")]
        [Route("~/")]
        [Route("~/Home")]
        public IActionResult Index()
        {
            return View();
        }
        //[Route("About")]
        //[Route("[action]")]
        public IActionResult About()
        {
            return View();
        }
        //[Route("Details/{id?}")]
        [Route("{id?}")]
        public int Details(int? id)
        {
            return id ?? 1;
        }
    }
}
