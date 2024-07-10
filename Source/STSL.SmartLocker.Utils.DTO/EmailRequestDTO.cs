using STSL.SmartLocker.Utils.Common.Enum;

namespace STSL.SmartLocker.Utils.DTO
{
    public class EmailRequestDTO
    {
        public string? ToEmail { get; set; } 
        public string? Subject { get; set; } 
        public string? PlainText { get; set; }
        public string? HtmlContent { get; set; }
        public EmailType EmailType { get; set; }
    };
}
