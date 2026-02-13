namespace CanAnalyzer.Models
{
    public class SdoType
    {
        public int SdoTypeId { get; set; }
        public string SdoTypeName { get; set; } = string.Empty;

        public ICollection<Sdo> Sdos { get; set; } = new List<Sdo>();
    }
}