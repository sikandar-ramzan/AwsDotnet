using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CSV_Modifier_Client.Models;
using CSV_Modifier_Client.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSV_Modifier_Client.Controllers
{
    public class AwsBucketReader : Controller
    {
        private readonly string bucketName = "csv-files-s3-bucket";

        //fetching this file (object) from s3 bucket
        private readonly string objectKey = "CommaSeparatedFile.csv";
        private readonly AwsSecretsService _awsSecretsService;

        public AwsBucketReader(AwsSecretsService awsSecretsService)
        {
            _awsSecretsService = awsSecretsService;
        }
        public async Task<IActionResult> Index()
        {
            //super user manager => superman
            string superman_access_key = await _awsSecretsService.GetAwsSecret("superman_access_key");
            string superman_secret_superman_access_key = await _awsSecretsService.GetAwsSecret("superman_secret_access_key");
            try
            {
                using var s3Client = new AmazonS3Client(superman_access_key, superman_secret_superman_access_key);

                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
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
                                ID = Convert.ToInt32(rowData[0]),
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
