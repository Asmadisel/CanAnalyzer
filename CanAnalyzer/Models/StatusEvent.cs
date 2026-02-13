namespace CanAnalyzer.Models
{
    public class StatusEvent
    {
        public Guid Id { get; set; }
        public DateTime Time { get; set; }
        public int StatusCode { get; set; }
        public int Sdo { get; set; }
        public DateTime RecordedAt { get; set; }

        public Status StatusNavigation { get; set; } = null!;
        public Sdo SdoNavigation { get; set; } = null!;
    }
}