namespace ExcelReaderAPI.Models
{

    public class ObjectResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = String.Empty;
    }

    public class UserAuthResponse : ObjectResponse
    {
        public User? UserFromDb { get; set; }
    }
}
