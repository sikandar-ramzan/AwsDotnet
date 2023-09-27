using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using ExcelDataReader;
using ExcelReaderAPI.Models;

namespace ExcelReaderAPI.Services
{


    public class FileUploadService
    {
        private readonly DatabaseHelper _databaseHelper;

        public FileUploadService(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public void AddUserFiles(Guid fileId, string filename, SqlConnection connection)
        {
            const string reqAuthor = "Sikandar R";
            var uploadDate = DateTime.Now;

            const string userInsertionQuery =
                "insert into ExcelReaderDb2.dbo.UserFiles values" +
                "(@fileId, @fileName, @author, @uploadDate)";

            var cmd = new SqlCommand(userInsertionQuery, connection);
            cmd.Parameters.AddWithValue("@fileId", fileId);
            cmd.Parameters.AddWithValue("@fileName", filename);
            cmd.Parameters.AddWithValue("@author", reqAuthor);
            cmd.Parameters.AddWithValue("@uploadDate", uploadDate);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }

        }

        public ObjectResponse UploadExcelFile(IFormFile file)
        {
            var sourceFileId = Guid.NewGuid();
            int rowsEffectCount = 0;

            using (var conn = new SqlConnection(_databaseHelper.GetDbConnString()))
            {
                conn.Open();
                const string checkFileExistQuery = "select COUNT('FILE_ID') from dbo.UserFiles where filename = @filename";

                var checkFileExistCmd = new SqlCommand(checkFileExistQuery, conn);

                checkFileExistCmd.Parameters.AddWithValue("@filename", file.FileName);
                var filesInDb = (Int32)checkFileExistCmd.ExecuteScalar();
                var fileAreadyInDb = filesInDb > 0;

                if (fileAreadyInDb) return new ObjectResponse { Success = false, Message = "File Already Present in DB" };

                try
                {
                    AddUserFiles(sourceFileId, file.FileName, conn);
                }
                catch
                {
                    return new ObjectResponse { Success = false, Message = "Error while adding user to database" };
                }

                var ITRequestsFromFile = ExtractDataFromExcelFile(file, sourceFileId);

                var cmd = new SqlCommand("Upload_IT_Request", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@RequestId", SqlDbType.UniqueIdentifier));
                cmd.Parameters.Add(new SqlParameter("@Author", SqlDbType.NVarChar, -1));
                cmd.Parameters.Add(new SqlParameter("@Type", SqlDbType.NVarChar, -1));
                cmd.Parameters.Add(new SqlParameter("@Subject", SqlDbType.NVarChar, -1));
                cmd.Parameters.Add(new SqlParameter("@Body", SqlDbType.NVarChar, -1));
                cmd.Parameters.Add(new SqlParameter("@SourceFileId", SqlDbType.UniqueIdentifier, -1));
                cmd.Parameters.Add(new SqlParameter("@Request_Date", SqlDbType.DateTime2));
                cmd.Parameters.Add(new SqlParameter("@Completion_Date", SqlDbType.DateTime2));
                cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, -1));

                try
                {
                    foreach (var itRequest in ITRequestsFromFile)
                    {
                        cmd.Parameters["@RequestId"].Value = itRequest.RequestId;
                        cmd.Parameters["@Author"].Value = itRequest.Author;
                        cmd.Parameters["@Type"].Value = itRequest.Type;
                        cmd.Parameters["@Subject"].Value = itRequest.Subject;
                        cmd.Parameters["@Body"].Value = itRequest.Body;
                        cmd.Parameters["@SourceFileId"].Value = itRequest.SourceFileId;
                        cmd.Parameters["@Request_Date"].Value = itRequest.RequestSubmissionDate;
                        cmd.Parameters["@Completion_Date"].Value = itRequest.RequestCompletionDate;
                        cmd.Parameters["@Status"].Value = itRequest.Status;

                        var response = cmd.ExecuteNonQuery();
                        rowsEffectCount += response;

                    }
                }
                catch
                {
                    return new ObjectResponse { Success = false, Message = "error while uploading it-request data to database" };
                }
            }
            return new ObjectResponse { Success = true, Message = $"{rowsEffectCount} rows added" };
        }

        public DateTime ConvertToDate(string? dateString)
        {
            string[] dateFormats = { "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy" };

            if (DateTime.TryParseExact(dateString, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }
            else
            {
                return new DateTime();
            }
        }

        public List<ITRequest> ExtractDataFromExcelFile(IFormFile file, Guid fileId)
        {

            var itRequests = new List<ITRequest>();

            using (var stream = file.OpenReadStream())
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        itRequests.Add(new ITRequest
                        {
                            RequestId = Guid.NewGuid(),
                            Author = reader.GetValue(1).ToString() ?? "",
                            Type = reader.GetValue(2).ToString() ?? "",
                            Subject = reader.GetValue(3).ToString() ?? "",
                            Body = reader.GetValue(4).ToString() ?? "",
                            SourceFileId = fileId,
                            RequestSubmissionDate = ConvertToDate(reader.GetValue(5).ToString()),
                            RequestCompletionDate = ConvertToDate(reader.GetValue(6).ToString()),
                            Status = reader.GetValue(7).ToString() ?? "",
                        });
                    }
                }
            }

            return itRequests;

        }

        public List<ITRequestWithFile> GetAllItRequests()
        {
            var itRequests = new List<ITRequestWithFile>();

            using (var connection = new SqlConnection(_databaseHelper.GetDbConnString()))
            {
                using (var command = new SqlCommand("IT_REQUESTS_WITH_FILE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var itRequest = new ITRequest
                            {
                                RequestId = (Guid)reader["RequestId"],
                                Author = reader["Author"].ToString() ?? "",
                                Type = reader["Type"].ToString() ?? "",
                                Subject = reader["Subject"].ToString() ?? "",
                                Body = reader["Body"].ToString() ?? "",
                                SourceFileId = (Guid)reader["SourceFileId"],
                                RequestSubmissionDate = (DateTime)reader["Request_Date"],
                                RequestCompletionDate = (DateTime)reader["Completion_Date"],
                                Status = reader["Status"].ToString() ?? ""
                            };

                            var userFile = new UserFile
                            {
                                FileId = (Guid)reader["File_Id"],
                                Filename = reader["Filename"].ToString() ?? "",
                                Owner = reader["Owner"].ToString() ?? "",
                                UploadDate = (DateTime)reader["UploadDate"]
                            };

                            var itRequestWithFile = new ITRequestWithFile
                            {
                                ITRequest = itRequest,
                                UserFile = userFile
                            };

                            itRequests.Add(itRequestWithFile);
                        }
                    }
                }
            }

            return itRequests;
        }

    }
}


