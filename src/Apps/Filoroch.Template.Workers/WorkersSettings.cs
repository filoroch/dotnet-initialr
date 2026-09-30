namespace Filoroch.Template.Workers;

public sealed class WorkersSettings
{
    public const string SectionName = "Workers";
    public int IntervaloSegundos { get; set; } = 300;
    public int DiasInatividade { get; set; } = 90;
}
