namespace Dangit.Telemetry

open System
open System.Collections.Generic
open System.Diagnostics
open System.Diagnostics.Metrics

/// Telemetry configuration
type TelemetryConfig = {
    Enabled: bool
    ConnectionString: string option
}

module TelemetryConfig =
    let disabled = { Enabled = false; ConnectionString = None }

    let create (enabled: bool) (connectionString: string option) =
        { Enabled = enabled; ConnectionString = connectionString }

/// Central telemetry service for dangit
module DangitTelemetry =
    let private sourceName = "Dangit"
    let private meterName = "Dangit.Metrics"

    // ActivitySource for tracing
    let activitySource = new ActivitySource(sourceName, "1.0.0")

    // Meter for metrics
    let meter = new Meter(meterName, "1.0.0")

    // Metrics
    let correctionsApplied = meter.CreateCounter<int64>("dangit.corrections.applied", "corrections", "Number of corrections applied")
    let rulesEvaluated = meter.CreateCounter<int64>("dangit.rules.evaluated", "rules", "Number of rules evaluated")
    let rulesMatched = meter.CreateCounter<int64>("dangit.rules.matched", "rules", "Number of rules that matched")
    let correctionLatency = meter.CreateHistogram<float>("dangit.correction.latency", "ms", "Time to find corrections")

    /// Record that a correction was applied
    let recordCorrection (ruleName: string) =
        correctionsApplied.Add(1L, KeyValuePair("rule", ruleName :> obj))

    /// Record rules evaluated
    let recordRulesEvaluated (count: int) =
        rulesEvaluated.Add(int64 count)

    /// Record rules matched
    let recordRulesMatched (count: int) =
        rulesMatched.Add(int64 count)

    /// Record correction latency in milliseconds
    let recordLatency (ms: float) =
        correctionLatency.Record(ms)

    /// Start a new activity span
    let startActivity (name: string) =
        activitySource.StartActivity(name)

    /// Measure and record the time to execute a function
    let timed (name: string) (f: unit -> 'a) : 'a =
        let sw = Stopwatch.StartNew()
        use _activity = startActivity name
        let result = f ()
        sw.Stop()
        recordLatency sw.Elapsed.TotalMilliseconds
        result
