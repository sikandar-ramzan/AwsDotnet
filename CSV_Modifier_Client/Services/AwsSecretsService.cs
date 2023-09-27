using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace CSV_Modifier_Client.Services
{
    public class AwsSecretsService
    {
        public async Task<string> GetAwsSecret (string secretName)
        {
            const string region = "us-east-1";
            var awsSecretsMngrClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT",
            };

            GetSecretValueResponse response;

            try
            {
                response = await awsSecretsMngrClient.GetSecretValueAsync(request);
            }
            catch 
            {
                throw new Exception("Error while fetching aws secret");
            }

            return response.SecretString;
        }
    }
}
