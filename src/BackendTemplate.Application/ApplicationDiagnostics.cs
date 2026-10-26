using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace BackendTemplate.Application;

public static class ApplicationDiagnostics
{
    public const string Name = "BackendTemplate.Application";
    public static readonly ActivitySource ActivitySource = new(Name);
    public static readonly Meter Meter = new(Name);
    public static readonly Histogram<double> RequestDuration =
        Meter.CreateHistogram<double>("application.request.duration", "ms");
}
