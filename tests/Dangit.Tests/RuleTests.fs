module Dangit.Tests.RuleTests

open Expecto
open Dangit.Core

let makeRule name matchFn getCmd =
    Rule.create name matchFn getCmd

[<Tests>]
let ruleTests =
    testList "Rule" [
        test "isMatch returns true when rule matches" {
            let rule = makeRule "test" (fun _ -> true) (fun _ -> ["fixed"])
            let cmd = Command.withOutput "bad" "error"
            Expect.isTrue (Rule.isMatch cmd rule) "Should match"
        }

        test "isMatch returns false when rule does not match" {
            let rule = makeRule "test" (fun _ -> false) (fun _ -> ["fixed"])
            let cmd = Command.withOutput "bad" "error"
            Expect.isFalse (Rule.isMatch cmd rule) "Should not match"
        }

        test "isMatch returns false when requires output but none provided" {
            let rule = makeRule "test" (fun _ -> true) (fun _ -> ["fixed"])
            let cmd = Command.withoutOutput "bad"
            Expect.isFalse (Rule.isMatch cmd rule) "Should not match without output"
        }

        test "isMatch with requiresOutput false matches without output" {
            let rule =
                makeRule "test" (fun _ -> true) (fun _ -> ["fixed"])
                |> Rule.withRequiresOutput false
            let cmd = Command.withoutOutput "bad"
            Expect.isTrue (Rule.isMatch cmd rule) "Should match without output when not required"
        }

        test "isMatch catches exceptions in match function" {
            let rule = makeRule "test" (fun _ -> failwith "boom") (fun _ -> ["fixed"])
            let cmd = Command.withOutput "bad" "error"
            Expect.isFalse (Rule.isMatch cmd rule) "Should return false on exception"
        }

        test "getCorrectedCommands returns commands with correct priorities" {
            let rule =
                makeRule "test" (fun _ -> true) (fun _ -> ["fix1"; "fix2"; "fix3"])
                |> Rule.withPriority 100
            let cmd = Command.withOutput "bad" "error"
            let corrected = Rule.getCorrectedCommands cmd rule
            Expect.equal (corrected |> List.map (fun c -> c.Priority)) [100; 200; 300] "Priorities should multiply"
        }

        test "getCorrectedCommands returns command scripts" {
            let rule = makeRule "test" (fun _ -> true) (fun _ -> ["fix1"; "fix2"])
            let cmd = Command.withOutput "bad" "error"
            let corrected = Rule.getCorrectedCommands cmd rule
            Expect.equal (corrected |> List.map (fun c -> c.Script)) ["fix1"; "fix2"] "Should return scripts"
        }

        test "withPriority changes priority" {
            let rule = makeRule "test" (fun _ -> true) (fun _ -> ["fix"]) |> Rule.withPriority 500
            Expect.equal rule.Priority 500 "Priority should be 500"
        }

        test "withEnabledByDefault changes default" {
            let rule = makeRule "test" (fun _ -> true) (fun _ -> ["fix"]) |> Rule.withEnabledByDefault false
            Expect.isFalse rule.EnabledByDefault "Should be disabled by default"
        }
    ]
