using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Monitoring;

public class MetricInstrumentation : IDisposable
{
    internal const string ActivitySourceName = "Customer Activity Source";
    internal const string MeterName = "CustomerMetric";
    private readonly Meter _meter;

    public ActivitySource ActivitySource { get; }
    public Counter<int> QueryCustomerCounter { get; }

    public MetricInstrumentation()
    {
        ActivitySource = new ActivitySource(ActivitySourceName);
        _meter = new Meter(MeterName);
        QueryCustomerCounter = _meter.CreateCounter<int>("customer.query.counter", "The counter of query");
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }
}
