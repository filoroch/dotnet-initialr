namespace Filoroch.Template.IoC.Settings;

public sealed class SwaggerSettings
{
    public const string SectionName = "Swagger";

    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = "API";
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "v1";
    public string RoutePrefix { get; set; } = "swagger";
    public string JsonRoute { get; set; } = "swagger/{documentName}/swagger.json";
}
