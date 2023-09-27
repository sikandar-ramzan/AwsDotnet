using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExcelReaderAPI.Models;
using Microsoft.IdentityModel.Tokens;
using ExcelReaderAPI.Utilities;

namespace ExcelReaderAPI.Services
{
    public class UserAuthService
    {

        private readonly DatabaseHelper _databaseHelper;
        private readonly IConfiguration _configuration;
        private readonly Helpers _helpers;


        public UserAuthService(IConfiguration configuration, DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
            _configuration = configuration;
            _helpers = new Helpers();
        }

        public ObjectResponse ValidateToken(string? token)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("AuthSecret:Token").Value);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            try
            {
                // Validate and parse the token
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                // Check if the token has expired
                var expiration = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
                if (!string.IsNullOrEmpty(expiration) && long.TryParse(expiration, out long expirationUnixTimestamp))
                {
                    var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expirationUnixTimestamp).UtcDateTime;
                    if (expirationDateTime <= DateTime.UtcNow)
                    {
                        return new ObjectResponse { Success = false, Message = "Token has expired." };
                    }
                }
            }
            catch
            {
                return new ObjectResponse { Success = false, Message = "Invalid Token" };
            }

            return new ObjectResponse { Success = true, Message = "Token is Valid" };
        }

        public string CreateToken(User user, bool isAdmin)
        {
            string userRole = isAdmin ? "Admin" : "User";
            List<Claim> claims = new List<Claim>
             {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, userRole),
                new Claim("UserId", user.UserId.ToString())
             };

            var authSecretToken = _configuration.GetSection("AuthSecret:Token").Value;
            int tokenExpirationInMinutes = _configuration.GetValue<int>("TokenExpirationTimeout:Minutes");
            if (authSecretToken == null)
            {
                throw new Exception("AuthSecret:Token configuration value is missing.");
            }
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(authSecretToken));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenExpiration = DateTime.Now.AddMinutes(tokenExpirationInMinutes);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: tokenExpiration,
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private User? GetUserFromDb(string username, SqlConnection connection)
        {
            string getUserQuery = "select * from Users where Username = @userName";

            var getUserCmd = _databaseHelper.CreateSqlCommand(getUserQuery, connection);
            getUserCmd.Parameters.AddWithValue("@userName", username);

            using (var reader = getUserCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = (Guid)reader["UserId"],
                        Username = (string)reader["Username"],
                        PasswordHash = (byte[])reader["PasswordHash"],
                        PasswordSalt = (byte[])reader["PasswordSalt"],
                        IsAdminUser = (bool)reader["IsAdminUser"]
                    };
                }
            }

            return null;
        }

        public ObjectResponse CreateUser(string username, string password, bool isAdmin)
        {
            var dbConnection = _databaseHelper.CreateDbConnection();
            dbConnection.Open();

            User? userFromDb = GetUserFromDb(username, dbConnection);

            if (userFromDb != null)
            {
                dbConnection.Dispose();
                return new ObjectResponse { Success = false, Message = $"User with username: {username} already exist!" };
            }


            _helpers.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsAdminUser = isAdmin
            };

            const string createUserQuery = "insert into Users values (@userId, @username, @pswrdHash, @pswrdSalt, @isAdmin)";

            var createUserCmd = _databaseHelper.CreateSqlCommand(createUserQuery, dbConnection);
            createUserCmd.Parameters.AddWithValue("@userId", user.UserId);
            createUserCmd.Parameters.AddWithValue("@userName", user.Username);
            createUserCmd.Parameters.AddWithValue("@pswrdHash", user.PasswordHash);
            createUserCmd.Parameters.AddWithValue("@pswrdSalt", user.PasswordSalt);
            createUserCmd.Parameters.AddWithValue("@isAdmin", user.IsAdminUser);

            var userCreationResponse = createUserCmd.ExecuteNonQuery();

            dbConnection.Dispose();

            if (userCreationResponse > 0)
            {
                return new ObjectResponse { Success = true, Message = $"User: {username} created successfully!" };
            }

            else
            {
                return new ObjectResponse { Success = false, Message = "Internal Server Error" };
            }
        }

        public UserAuthResponse LoginUser(string username, string password)
        {
            var dbConnection = _databaseHelper.CreateDbConnection();
            dbConnection.Open();

            User? userFromDb = GetUserFromDb(username, dbConnection);

            if (userFromDb == null)
            {
                dbConnection.Dispose();
                return new UserAuthResponse { Success = false, Message = $"User with username: {username} does not exist!" };
            }

            // Verify the entered password against the stored hash and salt
            if (_helpers.VerifyPasswordHash(password, userFromDb.PasswordHash, userFromDb.PasswordSalt))
            {
                return new UserAuthResponse { Success = true, Message = $"Login successful for user: {username}", UserFromDb = userFromDb };
            }

            dbConnection.Dispose();
            return new UserAuthResponse { Success = false, Message = "Invalid password" };
        }
    }
}
