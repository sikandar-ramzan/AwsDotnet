using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CSV_Modifier_Lambda;

public class Function
{
    /*IAmazonS3 S3Client { get; set; }*/
    private readonly IAmazonS3 _s3Client;
    private readonly Table _table;

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        /*        S3Client = new AmazonS3Client();*/
        _s3Client = new AmazonS3Client();
        var dynamoDBClient = new AmazonDynamoDBClient();
        _table = Table.LoadTable(dynamoDBClient, "csv_files_table");


    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client"></param>
    public Function(IAmazonS3 s3Client, Table table)
    {
        /*this.S3Client = s3Client;*/
        _s3Client = s3Client;
        _table = table;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records;

        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;

            if (s3Event == null)
            {
                continue;
            }

            try
            {
                // Read the CSV file from S3
                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = s3Event.Object.Key
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
                    var updatedKey = "Updated_" + s3Event.Object.Key;
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
                context.Logger.LogError($"Error processing object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}");
                context.Logger.LogError(e.Message);
                throw;
            }
        }
    }

}