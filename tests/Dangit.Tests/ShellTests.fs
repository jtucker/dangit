module Dangit.Tests.ShellTests

open Expecto
open Dangit.Core.Shells

[<Tests>]
let shellTests =
    testList "Shells" [
        testList "PowerShell" [
            let shell = PowerShellShell() :> IShell
            
            test "name is powershell" {
                Expect.equal shell.Name "powershell" "Should be powershell"
            }
            test "quote wraps in single quotes" {
                Expect.equal (shell.Quote "hello") "'hello'" "Should wrap"
            }
            test "quote escapes single quotes" {
                Expect.equal (shell.Quote "it's") "'it''s'" "Should escape"
            }
            test "split handles empty" {
                Expect.equal (shell.SplitCommand "") [] "Empty should return empty"
            }
            test "split handles basic command" {
                Expect.equal (shell.SplitCommand "git push") ["git"; "push"] "Should split"
            }
            test "and commands uses PowerShell syntax" {
                let result = shell.AndCommands "cmd1" "cmd2"
                Expect.stringContains result "cmd1" "Should contain cmd1"
                Expect.stringContains result "cmd2" "Should contain cmd2"
                Expect.stringContains result "$?" "Should use $?"
            }
            test "or commands uses PowerShell syntax" {
                let result = shell.OrCommands "cmd1" "cmd2"
                Expect.stringContains result "cmd1" "Should contain cmd1"
                Expect.stringContains result "cmd2" "Should contain cmd2"
            }
            test "generate alias contains function definition" {
                let alias = shell.GenerateAlias "dangit"
                Expect.stringContains alias "function dangit" "Should define function"
                Expect.stringContains alias "Get-History" "Should use Get-History"
            }
            test "generate alias uses custom name" {
                let alias = shell.GenerateAlias "oops"
                Expect.stringContains alias "function oops" "Should use custom name"
            }
        ]

        testList "Bash" [
            let shell = BashShell() :> IShell
            
            test "name is bash" {
                Expect.equal shell.Name "bash" "Should be bash"
            }
            test "quote wraps in single quotes" {
                Expect.equal (shell.Quote "hello") "'hello'" "Should wrap"
            }
            test "quote escapes single quotes" {
                Expect.equal (shell.Quote "it's") "'it'\\''s'" "Should escape for bash"
            }
            test "and commands uses &&" {
                Expect.equal (shell.AndCommands "cmd1" "cmd2") "cmd1 && cmd2" "Should use &&"
            }
            test "or commands uses ||" {
                Expect.equal (shell.OrCommands "cmd1" "cmd2") "cmd1 || cmd2" "Should use ||"
            }
            test "generate alias contains function" {
                let alias = shell.GenerateAlias "dangit"
                Expect.stringContains alias "function dangit" "Should define function"
                Expect.stringContains alias "fc -ln -1" "Should use fc for history"
            }
        ]

        testList "Zsh" [
            let shell = ZshShell() :> IShell
            
            test "name is zsh" {
                Expect.equal shell.Name "zsh" "Should be zsh"
            }
            test "and commands uses &&" {
                Expect.equal (shell.AndCommands "a" "b") "a && b" "Should use &&"
            }
            test "generate alias contains function" {
                let alias = shell.GenerateAlias "fix"
                Expect.stringContains alias "function fix" "Should define function"
            }
        ]

        testList "ShellDetector" [
            test "detect returns a shell" {
                let shell = ShellDetector.detect()
                Expect.isNotNull (shell.Name) "Should return a valid shell"
            }
            test "detected shell has non-empty name" {
                let shell = ShellDetector.detect()
                Expect.isNotEmpty shell.Name "Name should not be empty"
            }
        ]
    ]
