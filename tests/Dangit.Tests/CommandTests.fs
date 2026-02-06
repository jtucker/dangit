module Dangit.Tests.CommandTests

open Expecto
open Dangit.Core

[<Tests>]
let commandTests =
    testList "Command" [
        test "create splits script into parts" {
            let cmd = Command.create "git push origin main" None
            Expect.equal cmd.ScriptParts ["git"; "push"; "origin"; "main"] "Should split on spaces"
        }

        test "create handles empty script" {
            let cmd = Command.create "" None
            Expect.equal cmd.ScriptParts [] "Empty script should have no parts"
        }

        test "create handles whitespace-only script" {
            let cmd = Command.create "   " None
            Expect.equal cmd.ScriptParts [] "Whitespace-only should have no parts"
        }

        test "withOutput sets output" {
            let cmd = Command.withOutput "ls" "file1\nfile2"
            Expect.equal cmd.Output (Some "file1\nfile2") "Should have output"
        }

        test "withoutOutput has no output" {
            let cmd = Command.withoutOutput "ls"
            Expect.equal cmd.Output None "Should have no output"
        }

        test "create preserves original script" {
            let cmd = Command.create "git  push   origin" None
            Expect.equal cmd.Script "git  push   origin" "Should preserve original script"
        }
    ]
