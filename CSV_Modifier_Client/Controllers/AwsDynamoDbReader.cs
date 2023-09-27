using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CSV_Modifier_Client.Services;
using CSV_Modifier_Client.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSV_Modifier_Client.Controllers
{
    public class AwsDynamoDbReader : Controller
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;
        public AwsDynamoDbReader(AwsSecretsService awsSecretsService, IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }
        public IActionResult Index()
        {
            string tableName = "csv_files_table";

            var scanRequest = new ScanRequest
            {
                TableName = tableName,
               
            };

            //will scan and return all items from dynam db -skndr
            var scanResponse = _dynamoDBClient.ScanAsync(scanRequest).Result;

            var items = scanResponse.Items.Select(item =>
                 new DynamoDbItem
                 {
                     ID = item["ID"].S,
                     Name = item["Name"].S,
                     TechStack = item["Tech_Stack"].S
                 }).ToList();

            return View(items);
        }
    }
}
