namespace CanAnalyzer.Services;

public interface IEventGenerator
{
    void Enable();
    void Disable();
    bool IsEnabled();
}