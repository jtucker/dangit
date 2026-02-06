namespace Dangit.Rules

open Dangit.Core

module GitPull =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git push") &&
        match cmd.Output with
        | Some output ->
            output.Contains("rejected") &&
            (output.Contains("non-fast-forward") || output.Contains("fetch first"))
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ sprintf "git pull && %s" cmd.Script ]

    let rule =
        Rule.create "git_pull" matchCmd getNewCommand
