using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExcelReaderAPI.Models
{
    public class ITRequest
    {
        [Key]
        public Guid RequestId { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        [ForeignKey("SourceFileId")]
        public Guid SourceFileId { get; set; }
        public DateTime RequestSubmissionDate { get; set; }
        public DateTime RequestCompletionDate { get; set; }
        public string Status { get; set; } = string.Empty;


    }
}
