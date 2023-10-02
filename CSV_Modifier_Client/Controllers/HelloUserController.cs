using CSV_Modifier_Client.Core;
using CSV_Modifier_Client.Models;
using CSV_Modifier_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSV_Modifier_Client.Controllers
{
    public class HelloUserController : Controller
    {
        private readonly AwsSecretsService _awsSecretsService;

        public HelloUserController(AwsSecretsService awsSecretsService)
        {
            _awsSecretsService = awsSecretsService;
        }
        public async Task<IActionResult> Index()
        {
            const string secretName = Constants.SuperUserNameSecret;
            string awsSecret = await _awsSecretsService.GetAwsSecret(secretName);

            return View(new HelloUserModel { Token = awsSecret });
        }
    }
}
