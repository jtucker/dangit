namespace Dangit.Rules

open Dangit.Core

module GitStash =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git ") &&
        match cmd.Output with
        | Some output ->
            (output.Contains("Your local changes") || output.Contains("local changes")) &&
            (output.Contains("would be overwritten") || output.Contains("Please commit or stash"))
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ sprintf "git stash && %s && git stash pop" cmd.Script ]

    let rule =
        Rule.create "git_stash" matchCmd getNewCommand
