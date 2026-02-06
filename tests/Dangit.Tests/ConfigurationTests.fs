module Dangit.Tests.ConfigurationTests

open Expecto
open Dangit.Core

[<Tests>]
let configTests =
    testList "Configuration" [
        test "default settings has ALL rules enabled" {
            let settings = Configuration.defaultSettings
            Expect.contains settings.Rules "ALL" "Should contain ALL"
        }

        test "default alias is dangit" {
            let settings = Configuration.defaultSettings
            Expect.equal settings.AliasName "dangit" "Default alias should be dangit"
        }

        test "isRuleEnabled returns true for enabled rule with ALL" {
            let settings = Configuration.defaultSettings
            let rule = Rule.create "test" (fun _ -> true) (fun _ -> ["fix"])
            Expect.isTrue (Configuration.isRuleEnabled settings rule) "Should be enabled"
        }

        test "isRuleEnabled returns false for excluded rule" {
            let settings = { Configuration.defaultSettings with ExcludeRules = ["test"] }
            let rule = Rule.create "test" (fun _ -> true) (fun _ -> ["fix"])
            Expect.isFalse (Configuration.isRuleEnabled settings rule) "Should be excluded"
        }

        test "isRuleEnabled returns false for disabled-by-default with ALL" {
            let settings = Configuration.defaultSettings
            let rule =
                Rule.create "test" (fun _ -> true) (fun _ -> ["fix"])
                |> Rule.withEnabledByDefault false
            Expect.isFalse (Configuration.isRuleEnabled settings rule) "Disabled by default not included in ALL"
        }

        test "isRuleEnabled returns true for explicitly listed rule" {
            let settings = { Configuration.defaultSettings with Rules = ["test"] }
            let rule = Rule.create "test" (fun _ -> true) (fun _ -> ["fix"])
            Expect.isTrue (Configuration.isRuleEnabled settings rule) "Should be enabled when explicitly listed"
        }

        test "isRuleEnabled returns false for unlisted rule without ALL" {
            let settings = { Configuration.defaultSettings with Rules = ["other"] }
            let rule = Rule.create "test" (fun _ -> true) (fun _ -> ["fix"])
            Expect.isFalse (Configuration.isRuleEnabled settings rule) "Should not be enabled when not listed"
        }

        test "default wait command is 3" {
            Expect.equal Configuration.defaultSettings.WaitCommand 3 "Default wait should be 3"
        }
    ]
