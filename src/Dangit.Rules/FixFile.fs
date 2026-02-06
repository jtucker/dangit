namespace Dangit.Rules

open Dangit.Core

module FixFile =
    let private matchCmd (cmd: Command) =
        match cmd.Output with
        | Some output ->
            output.ToLowerInvariant().Contains("permission denied") &&
            not (cmd.Script.StartsWith("sudo ")) &&
            cmd.ScriptParts.Length = 1
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ sprintf "chmod +x %s && %s" cmd.Script cmd.Script ]

    let rule =
        Rule.create "fix_file" matchCmd getNewCommand
