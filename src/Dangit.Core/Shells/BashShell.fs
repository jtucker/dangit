namespace Dangit.Core.Shells

open System
open Dangit.Core

type BashShell() =
    interface IShell with
        member _.Name = "bash"

        member _.GetLastCommand() = None

        member _.SplitCommand(script: string) =
            if String.IsNullOrWhiteSpace(script) then []
            else
                script.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                |> Array.toList

        member _.Quote(s: string) =
            sprintf "'%s'" (s.Replace("'", "'\\''"))

        member _.GenerateAlias(aliasName: string) =
            "function " + aliasName + " () {\n" +
            "    local last_cmd=$(fc -ln -1)\n" +
            "    local last_output=$(eval \"$last_cmd\" 2>&1)\n" +
            "    dangit --command \"$last_cmd\" --output \"$last_output\"\n" +
            "}\n" +
            "alias " + aliasName + "='" + aliasName + "'"

        member _.AndCommands(cmd1: string) (cmd2: string) =
            sprintf "%s && %s" cmd1 cmd2

        member _.OrCommands(cmd1: string) (cmd2: string) =
            sprintf "%s || %s" cmd1 cmd2

        member _.PutToHistory(cmd: string) =
            // Would use `history -s` in real bash
            ignore cmd
