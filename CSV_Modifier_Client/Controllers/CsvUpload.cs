using Amazon.S3.Transfer;
using Amazon.S3;
using CSV_Modifier_Client.Models;
using Microsoft.AspNetCore.Mvc;
using Amazon;
using CSV_Modifier_Client.Services;

namespace CSV_Modifier_Client.Controllers
{
    public class CsvUpload : Controller
    {
        private readonly string bucketName = "csv-files-s3-bucket";
        private readonly AwsSecretsService _awsSecretsService;
        public CsvUpload(AwsSecretsService awsSecretsService)
        {
            _awsSecretsService = awsSecretsService;
        }
        
        public IActionResult Index()
        {
            var model = new UploadViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("csvFile", "Please select a CSV file.");
                return View("Index");
            }
            string superman_access_key = await _awsSecretsService.GetAwsSecret("superman_access_key");
            string superman_secret_superman_access_key = await _awsSecretsService.GetAwsSecret("superman_secret_access_key");
            try
            {
                using var s3Client = new AmazonS3Client(superman_access_key, superman_secret_superman_access_key);

                using var memoryStream = new MemoryStream();
                await csvFile.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var fileTransferUtility = new TransferUtility(s3Client);
                await fileTransferUtility.UploadAsync(memoryStream, bucketName, csvFile.FileName);

                var model = new UploadViewModel
                {
                    Message = "CSV file uploaded successfully to S3"
                };

                return View("Index", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("csvFile", $"Error: {ex.Message}");
                return View("Index");
            }
        }
    }
}
