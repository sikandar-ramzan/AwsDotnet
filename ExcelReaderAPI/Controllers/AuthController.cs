using ExcelReaderAPI.Models;
using ExcelReaderAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcelReaderAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserAuthService _authServices;

        public AuthController(UserAuthService authServices)
        {
            _authServices = authServices;
        }

        [HttpPost("register-user")]
        public IActionResult RegisterUser(UserDto request)
        {
            var userCreationResponse = _authServices.CreateUser(request.Username, request.Password, false);

            if (userCreationResponse.Success)
            {
                return Ok($"User: {request.Username} created successfully!");
            }

            return BadRequest(userCreationResponse.Message);
        }

        [HttpPost("register-admin")]
        public ActionResult RegisterAdminUser(UserDto request)
        {
            var userCreationResponse = _authServices.CreateUser(request.Username, request.Password, true);
            if (userCreationResponse.Success)
            {
                return Ok(userCreationResponse.Message);
            }

            return BadRequest(userCreationResponse.Message);
        }

        [HttpPost("login")]
        public ActionResult<object> Login(UserDto request)
        {
            var userLoginResponse = _authServices.LoginUser(request.Username, request.Password);
            if (!userLoginResponse.Success || userLoginResponse.UserFromDb == null)
            {
                return BadRequest(userLoginResponse.Message);
            }

            var userFromDb = userLoginResponse.UserFromDb;
            var token = _authServices.CreateToken(userFromDb, userFromDb.IsAdminUser);

            return Ok(new { Token = token, UserId = userLoginResponse.UserFromDb.UserId, Username = userLoginResponse.UserFromDb.Username, IsAdminUser = userLoginResponse.UserFromDb.IsAdminUser });
        }

    }

}
