using ExcelReader.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelReader.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<ItRequest> ItRequests { get; set; }
        public DbSet<UserFile> UserFiles { get; set; }
    }
}
