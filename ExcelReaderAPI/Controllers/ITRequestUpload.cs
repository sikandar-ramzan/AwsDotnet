using ExcelReaderAPI.Models;
using ExcelReaderAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExcelReaderAPI.Controllers
{
    [Route("it-request")]
    [ApiController]
    public class ITRequestUpload : ControllerBase
    {
        private readonly FileUploadService _fileUploadService;
        private readonly UserAuthService _authService;

        public ITRequestUpload(FileUploadService fileUploadService, UserAuthService authServices)
        {
            _fileUploadService = fileUploadService;
            _authService = authServices;
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet]
        public ActionResult<List<ITRequestWithFile>> GetAllITRequests()
        {

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var isTokenVaild = _authService.ValidateToken(token);

            if (!isTokenVaild.Success)
            {
                return Unauthorized(isTokenVaild.Message);
            }

            return _fileUploadService.GetAllItRequests();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public IActionResult UploadExcelFile(IFormFile file)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var isTokenVaild = _authService.ValidateToken(token);

            if (!isTokenVaild.Success)
            {
                return Unauthorized("User Unauthorized");
            }



            if (file != null && file.Length > 0)
            {
                var uploadResponse = _fileUploadService.UploadExcelFile(file);

                if (uploadResponse.Success)
                {
                    return Ok(uploadResponse.Message);
                }

                else
                {
                    return BadRequest(uploadResponse.Message);
                }
            }

            return BadRequest("file not valid");
        }
    }
}