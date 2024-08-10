using Microsoft.AspNetCore.Mvc;

namespace Controllers_Actions.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            TempData["Name"] = "Devarsh";
            TempData.Keep();
            return View();
        }
        public IActionResult About()
        {
            TempData.Keep();
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
//View Bag
//ViewBag.Data1 = "Home";
//ViewBag.Data2 = 24;
//ViewBag.Data3 = DateTime.Now.ToShortDateString();

//string[] arr = { "A", "B", "C" };
//ViewBag.Data4 = arr;

//ViewBag.Data5 = new List<string>()
//{
//    "A", "B", "C"
//};
//ViewData["Name"] = "Name";
//ViewBag.Data6 = "Hii";
//View Data
//ViewData["Data"] = "Devarsh";
//ViewData["Data1"] = 23;
//ViewData["Data2"] = DateTime.Now.ToLongDateString();

//string[] arr = { "A", "B", "C" };
//ViewData["Data3"] = arr;
//ViewData["Data4"] = new List<string>()
//{
//    "A","B","C"
//};