namespace CanAnalyzer.Models
{
    public class Status
    {
        public int StatusCodeId { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();
    }
}
