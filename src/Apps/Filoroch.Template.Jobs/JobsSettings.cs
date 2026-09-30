namespace Filoroch.Template.Jobs;

public sealed class JobsSettings
{
    public const string SectionName = "Jobs";
    public string Cron { get; set; } = "0 0 3 * * ?";
    public int DiasInatividade { get; set; } = 90;
}
