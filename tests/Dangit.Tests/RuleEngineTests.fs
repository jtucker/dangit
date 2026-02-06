module Dangit.Tests.RuleEngineTests

open Expecto
open Dangit.Core

[<Tests>]
let ruleEngineTests =
    testList "RuleEngine" [
        test "returns corrections from matching rules" {
            let rule1 = Rule.create "r1" (fun _ -> true) (fun _ -> ["fix1"])
            let rule2 = Rule.create "r2" (fun _ -> true) (fun _ -> ["fix2"])
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [rule1; rule2] cmd
            Expect.equal (corrections |> List.map (fun c -> c.Script)) ["fix1"; "fix2"] "Should return all matches"
        }

        test "filters out non-matching rules" {
            let rule1 = Rule.create "r1" (fun _ -> true) (fun _ -> ["fix1"])
            let rule2 = Rule.create "r2" (fun _ -> false) (fun _ -> ["fix2"])
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [rule1; rule2] cmd
            Expect.equal (corrections |> List.map (fun c -> c.Script)) ["fix1"] "Should only return matching"
        }

        test "deduplicates by script" {
            let rule1 = Rule.create "r1" (fun _ -> true) (fun _ -> ["fix"])
            let rule2 = Rule.create "r2" (fun _ -> true) (fun _ -> ["fix"])
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [rule1; rule2] cmd
            Expect.equal corrections.Length 1 "Should deduplicate"
        }

        test "sorts by priority" {
            let rule1 = Rule.create "r1" (fun _ -> true) (fun _ -> ["fix1"]) |> Rule.withPriority 2000
            let rule2 = Rule.create "r2" (fun _ -> true) (fun _ -> ["fix2"]) |> Rule.withPriority 500
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [rule1; rule2] cmd
            Expect.equal (corrections |> List.map (fun c -> c.Script)) ["fix2"; "fix1"] "Should sort by priority"
        }

        test "returns empty list when no rules match" {
            let rule = Rule.create "r1" (fun _ -> false) (fun _ -> ["fix"])
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [rule] cmd
            Expect.isEmpty corrections "Should be empty"
        }

        test "handles empty rule list" {
            let cmd = Command.withOutput "bad" "error"
            let corrections = RuleEngine.getCorrectedCommands [] cmd
            Expect.isEmpty corrections "Should be empty with no rules"
        }
    ]
