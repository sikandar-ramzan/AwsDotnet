using ExcelReader.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExcelReader.Controllers
{
    public class ViewITRequestsController : Controller
    {
        private readonly IFileUploadService _fileUploadService;

        public ViewITRequestsController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        public async Task<IActionResult> Index()
        {
            var itRequests = await _fileUploadService.GetExcelFileData();
            return View(itRequests);
        }
    }
}
