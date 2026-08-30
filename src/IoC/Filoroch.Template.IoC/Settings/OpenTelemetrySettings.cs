namespace Filoroch.Template.IoC.Settings;

public sealed class OpenTelemetrySettings
{
    public const string SectionName = "OpenTelemetry";
    public string ServiceName { get; set; } = "Filoroch.Template";
    public string Endpoint { get; set; } = "http://localhost:4317";
}
