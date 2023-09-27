using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    [Route("api/signin")]
    [ApiController]
    public class SignInController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
