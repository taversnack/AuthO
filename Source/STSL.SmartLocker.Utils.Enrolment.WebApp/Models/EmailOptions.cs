namespace STSL.SmartLocker.Utils.Enrolment.WebApp.Models
{
    public class EmailOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new List<string>();
    }
}
