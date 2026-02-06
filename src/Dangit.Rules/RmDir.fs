namespace Dangit.Rules

open Dangit.Core

module RmDir =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("rm ") &&
        not (cmd.Script.Contains("-r")) &&
        match cmd.Output with
        | Some output ->
            output.ToLowerInvariant().Contains("is a directory")
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ cmd.Script.Replace("rm ", "rm -rf ") ]

    let rule =
        Rule.create "rm_dir" matchCmd getNewCommand
