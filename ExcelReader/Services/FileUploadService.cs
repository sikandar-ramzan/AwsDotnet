using ExcelReader.Data;
using ExcelReader.Models;
using Microsoft.EntityFrameworkCore;


namespace ExcelReader.Services
{
    public class FileUploadService
    {
        private readonly DataContext _dbContext;

        public FileUploadService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<ItRequest>> GetExcelFileData()
        {
            return await _dbContext.ItRequests.ToListAsync();
        }
    }
}

