namespace ExcelReader.Models
{
    public interface IFileUploadService
    {
        Task<bool> UploadExcelFile(IFormFile file);
        Task<List<ItRequest>> GetExcelFileData();
        DateTime ConvertToDate(string dateString);
        Task<List<ITRequestWithFile>> GetITRequestsWithFiles();
    }
}
