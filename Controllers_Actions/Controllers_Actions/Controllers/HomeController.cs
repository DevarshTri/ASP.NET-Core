using Microsoft.AspNetCore.Mvc;

namespace Controllers_Actions.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Data"] = "Devarsh";
            ViewData["Data1"] = 23;
            ViewData["Data2"] = DateTime.Now.ToLongDateString();

            string[] arr = { "A", "B", "C" };
            ViewData["Data3"] = arr;
            ViewData["Data4"] = new List<string>()
            {
                "A","B","C"
            };
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
