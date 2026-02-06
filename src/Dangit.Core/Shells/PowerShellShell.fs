namespace Dangit.Core.Shells

open System
open Dangit.Core

type PowerShellShell() =
    interface IShell with
        member _.Name = "powershell"

        member _.GetLastCommand() =
            // In real usage, this reads from the shell function that passes args
            // For now, return None — the CLI will pass command via args
            None

        member _.SplitCommand(script: string) =
            if String.IsNullOrWhiteSpace(script) then []
            else
                script.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                |> Array.toList

        member _.Quote(s: string) =
            sprintf "'%s'" (s.Replace("'", "''"))

        member _.GenerateAlias(aliasName: string) =
            "function " + aliasName + " {\n" +
            "    $lastCmd = (Get-History -Count 1).CommandLine\n" +
            "    $lastOutput = $Error[0] | Out-String\n" +
            "    dangit --command $lastCmd --output $lastOutput\n" +
            "}\n"

        member _.AndCommands(cmd1: string) (cmd2: string) =
            sprintf "%s; if ($?) { %s }" cmd1 cmd2

        member _.OrCommands(cmd1: string) (cmd2: string) =
            sprintf "%s; if (-not $?) { %s }" cmd1 cmd2

        member _.PutToHistory(_cmd: string) =
            // PowerShell doesn't easily support programmatic history insertion
            ()
