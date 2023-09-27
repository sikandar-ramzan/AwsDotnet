using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    public class WeatherForecastController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
