namespace Filoroch.Template.IoC.Settings;

public sealed class OpenTelemetrySettings
{
    public const string SectionName = "OpenTelemetry";
    public string ServiceName { get; set; } = "Filoroch.Template";
    public string Endpoint { get; set; } = "http://localhost:4317";
    public string Protocol { get; set; } = "http/protobuf";
    public string Headers { get; set; } = string.Empty;
    public string ResourceAttributes { get; set; } = string.Empty;
}
