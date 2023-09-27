using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    [Route("/it-request")]
    public class ItRequestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }


}

