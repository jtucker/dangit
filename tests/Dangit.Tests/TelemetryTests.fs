module Dangit.Tests.TelemetryTests

open Expecto
open Dangit.Telemetry

[<Tests>]
let telemetryTests =
    testList "Telemetry" [
        testList "TelemetryConfig" [
            test "disabled config has enabled=false" {
                let config = TelemetryConfig.disabled
                Expect.isFalse config.Enabled "Should be disabled"
            }
            test "disabled config has no connection string" {
                let config = TelemetryConfig.disabled
                Expect.isNone config.ConnectionString "Should have no connection string"
            }
            test "create with enabled and connection string" {
                let config = TelemetryConfig.create true (Some "InstrumentationKey=test")
                Expect.isTrue config.Enabled "Should be enabled"
                Expect.equal config.ConnectionString (Some "InstrumentationKey=test") "Should have connection string"
            }
            test "create disabled without connection string" {
                let config = TelemetryConfig.create false None
                Expect.isFalse config.Enabled "Should be disabled"
            }
        ]

        testList "DangitTelemetry" [
            test "activitySource has correct name" {
                Expect.equal DangitTelemetry.activitySource.Name "Dangit" "Source name should be Dangit"
            }
            test "meter has correct name" {
                Expect.equal DangitTelemetry.meter.Name "Dangit.Metrics" "Meter name should be Dangit.Metrics"
            }
            test "recordCorrection does not throw" {
                DangitTelemetry.recordCorrection "test_rule"
            }
            test "recordRulesEvaluated does not throw" {
                DangitTelemetry.recordRulesEvaluated 5
            }
            test "recordRulesMatched does not throw" {
                DangitTelemetry.recordRulesMatched 2
            }
            test "recordLatency does not throw" {
                DangitTelemetry.recordLatency 42.5
            }
            test "timed returns result and records" {
                let result = DangitTelemetry.timed "test" (fun () -> 42)
                Expect.equal result 42 "Should return function result"
            }
            test "startActivity returns activity or null" {
                let activity = DangitTelemetry.startActivity "test"
                if activity <> null then activity.Dispose()
            }
        ]

        testList "TelemetrySetup" [
            test "initialize with disabled config does not throw" {
                TelemetrySetup.initialize TelemetryConfig.disabled
            }
            test "shutdown does not throw when not initialized" {
                TelemetrySetup.shutdown()
            }
        ]
    ]
