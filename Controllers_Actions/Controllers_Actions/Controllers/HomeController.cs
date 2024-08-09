using Microsoft.AspNetCore.Mvc;

namespace Controllers_Actions.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }
        //public string Display()
        //{
        //    return "Devarsh Trivedi";
        //}
        //public int DisplayId(int id)
        //{
        //    return id;
        //}

        ////Content Result
        //public ContentResult ContentResult()
        //{
        //    return Content("Content Result");
        //}

        ////Json Result
        //public JsonResult JsonResult()
        //{
        //    var name = "Devarsh Trivedi";
        //    //return Json(new {data = name });
        //    return Json(name);
        //}

        ////Partial View Result
        //public PartialViewResult PartialViewResult()
        //{
        //    return PartialViewResult("_PartialView");
        //}
    }
}
