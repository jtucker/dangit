namespace Dangit.Telemetry

open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace
open OpenTelemetry.Resources
open Azure.Monitor.OpenTelemetry.Exporter

/// Setup and lifecycle management for OpenTelemetry
module TelemetrySetup =
    let mutable private meterProvider: MeterProvider option = None
    let mutable private tracerProvider: TracerProvider option = None

    /// Initialize OpenTelemetry with the given configuration
    let initialize (config: TelemetryConfig) =
        if not config.Enabled then ()
        else
            let resourceBuilder =
                ResourceBuilder.CreateDefault()
                    .AddService("dangit", serviceVersion = "1.0.0")

            // Setup metrics
            let metricsBuilder =
                Sdk.CreateMeterProviderBuilder()
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter("Dangit.Metrics")

            // Setup tracing
            let tracingBuilder =
                Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource("Dangit")

            // Add Azure Monitor exporter if connection string provided,
            // otherwise use console exporter for local debugging
            let metricsBuilder, tracingBuilder =
                match config.ConnectionString with
                | Some connStr ->
                    let metricsWithExporter =
                        metricsBuilder.AddAzureMonitorMetricExporter(fun opts ->
                            opts.ConnectionString <- connStr)
                    let tracingWithExporter =
                        tracingBuilder.AddAzureMonitorTraceExporter(fun opts ->
                            opts.ConnectionString <- connStr)
                    metricsWithExporter, tracingWithExporter
                | None ->
                    let metricsWithConsole = metricsBuilder.AddConsoleExporter()
                    let tracingWithConsole = tracingBuilder.AddConsoleExporter()
                    metricsWithConsole, tracingWithConsole

            meterProvider <- Some (metricsBuilder.Build())
            tracerProvider <- Some (tracingBuilder.Build())

    /// Shutdown telemetry providers
    let shutdown () =
        meterProvider |> Option.iter (fun p -> p.Dispose())
        tracerProvider |> Option.iter (fun p -> p.Dispose())
        meterProvider <- None
        tracerProvider <- None
