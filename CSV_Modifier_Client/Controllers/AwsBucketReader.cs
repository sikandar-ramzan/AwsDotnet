using Amazon.S3;
using Amazon.S3.Model;
using CSV_Modifier_Client.Core;
using CSV_Modifier_Client.Models;
using CSV_Modifier_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace CSV_Modifier_Client.Controllers
{
    public class AwsBucketReader : Controller
    {
        private const string BucketName = Constants.BucketName;

        //fetching this file (object) from s3 bucket
        private readonly string ObjectKey = Constants.ObjectKey;
        private readonly AwsSecretsService _awsSecretsService;

        public AwsBucketReader(AwsSecretsService awsSecretsService)
        {
            _awsSecretsService = awsSecretsService;
        }
        public async Task<IActionResult> Index()
        {
            //super user manager => superman
            var supermanAccessKey = await _awsSecretsService.GetAwsSecret(Constants.SuperUserAccessKey);
            var supermanSecretAccessKey = await _awsSecretsService.GetAwsSecret(Constants.SuperUserSectretKey);
            try
            {
                using var s3Client = new AmazonS3Client(supermanAccessKey, supermanSecretAccessKey);

                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = BucketName,
                    Key = ObjectKey
                };

                using var response = await s3Client.GetObjectAsync(getObjectRequest);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    using var reader = new StreamReader(response.ResponseStream);
                    var csvModels = new List<CsvDataModel>();

                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var rowData = line.Split(',');
                        if (rowData.Length >= 3)
                        {
                            var model = new CsvDataModel
                            {
                                Id = Convert.ToInt32(rowData[0]),
                                Name = rowData[1],
                                TechStack = rowData[2]
                            };
                            csvModels.Add(model);
                        }
                    }

                    return View("Index", csvModels);
                }
                else
                {
                    return Content("Failed to retrieve CSV file from S3");
                }
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }
    }
}
