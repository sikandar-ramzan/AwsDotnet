using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using CSV_Modifier_Client.Models;

namespace CSV_Modifier_Client.Services
{
    public class DynamoDbService
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;

        public DynamoDbService(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }

        public async Task<List<DynamoDbItem>> GetAllItemsAsync()
        {
            var context = new DynamoDBContext(_dynamoDBClient);

            try
            {
                // providing no filter condition to fetch all items
                var scanConditions = new List<ScanCondition>();

                var results = await context.ScanAsync<DynamoDbItem>(scanConditions).GetRemainingAsync();
                return results;
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
