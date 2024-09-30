using System;
using System.Diagnostics;
using Dafda.Diagnostics;
using OpenTelemetry.Context.Propagation;

public class DafdaActivitySourceFixture : IDisposable
{
    public DafdaActivitySourceFixture()
    {
        // Reset the static state before each test class
        ResetDafdaActivitySource();
    }

    public void ResetDafdaActivitySource()
    {
        DafdaActivitySource.Propagator = Propagators.DefaultTextMapPropagator;

        DafdaActivitySource.CustomActivityNameFuncs = new();
        // Stop and dispose of any current activity
        while (Activity.Current != null)
        {
            Activity.Current?.Stop();
            Activity.Current?.Dispose();
        }
    }

    public void Dispose()
    {
        // Reset the static state after each test class
        ResetDafdaActivitySource();
    }
}