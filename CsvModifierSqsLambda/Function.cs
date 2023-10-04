using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CsvModifierSqsLambda;

public class Function
{
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    /// 
    private readonly IAmazonS3 _s3Client;
    private readonly Table _table;
    private readonly IAmazonSQS _sqsClient;  // Add SQS client

    public Function()
    {
        _s3Client = new AmazonS3Client();
        var dynamoDBClient = new AmazonDynamoDBClient();
        _table = Table.LoadTable(dynamoDBClient, "csv_files_table");

        // Initialize the SQS client
        _sqsClient = new AmazonSQSClient();

    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
     
        var messageBody = evnt.Records[0].Body;  // Assuming only one message at a time

        try
        {
            // Read the CSV file from S3
            var getObjectRequest = new GetObjectRequest
            {
                BucketName = "csv-files-s3-bucket",
                Key = $"{messageBody}.csv" // CSV file name is in the message body
            };


            using (var response = await _s3Client.GetObjectAsync(getObjectRequest))
            using (var reader = new StreamReader(response.ResponseStream))
            {
                var data = await reader.ReadToEndAsync();
                var employees = data.Split('\n');

                // Modify the CSV data and add "dataInsertedIntoDB"
                var updatedData = new List<string>();
                foreach (var employee in employees)
                {
                    var employeeData = employee.Split(',');
                    if (employeeData.Length == 3)
                    {
                        updatedData.Add(string.Join(",", employeeData.Append("dataInsertedIntoDB")));
                    }
                }

                var updatedCsv = string.Join("\n", updatedData);

                // Save the updated CSV back to S3
                var updatedKey = "Updated_" + getObjectRequest.Key;
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = "serverless-fileupload-app",
                    Key = updatedKey,
                    ContentBody = updatedCsv
                };

                await _s3Client.PutObjectAsync(putObjectRequest);

                // Insert data into DynamoDB
                foreach (var employee in employees)
                {
                    var employeeData = employee.Split(',');
                    try
                    {
                        var document = new Document
                        {
                            ["ID"] = employeeData[0],
                            ["Name"] = employeeData[1],
                            ["Tech_Stack"] = employeeData[2]
                        };

                        await _table.PutItemAsync(document);
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogError("Error inserting data into DynamoDB");
                        context.Logger.LogError(e.Message);
                    }
                }
            }

        }
        catch (Exception e)
        {
            context.Logger.LogError($"Error processing object {messageBody} from bucket csv-files-s3-bucket");
            context.Logger.LogError(e.Message);
            throw;
        }
    }

   /* private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed message {message.Body}");

        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }*/
}