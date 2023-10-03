using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;

namespace CSV_Modifier_Client.Services
{
    public class AwsSecretsService
    {
        private readonly IConfiguration _configuration;

        public AwsSecretsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GetAwsSecret (string secretName)
        {
            
            var awsSecretsMngrClient = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_configuration["AWSConfig:Region"]));

            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = _configuration["AWSConfig:VersionStage"]
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
