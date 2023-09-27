using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    [Route("api/signup")]
    [ApiController]
    public class SignUpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
