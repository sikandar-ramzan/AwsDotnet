using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    public class ITRequestWithFileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
