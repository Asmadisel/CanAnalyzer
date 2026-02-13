using System.ComponentModel.DataAnnotations.Schema;

namespace CanAnalyzer.Models;

public class Sdo
{
    public int SdoId { get; set; }
    public int Number { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    [ForeignKey("SdoTypeNavigation")]
    public int SdoType { get; set; }

    public SdoType SdoTypeNavigation { get; set; } = null!;
    public ICollection<StatusEvent> StatusEvents { get; set; } = new List<StatusEvent>();
}