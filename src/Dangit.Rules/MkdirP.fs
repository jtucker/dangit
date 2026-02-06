namespace Dangit.Rules

open Dangit.Core

module MkdirP =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("mkdir ") &&
        not (cmd.Script.Contains("-p")) &&
        match cmd.Output with
        | Some output ->
            let lower = output.ToLowerInvariant()
            lower.Contains("no such file or directory") ||
            lower.Contains("cannot create directory")
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ cmd.Script.Replace("mkdir ", "mkdir -p ") ]

    let rule =
        Rule.create "mkdir_p" matchCmd getNewCommand
