namespace Dangit.Rules

open Dangit.Core

module GitCommitAmend =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git commit") &&
        not (cmd.Script.Contains("--amend")) &&
        match cmd.Output with
        | Some output ->
            output.Contains("nothing to commit") ||
            output.Contains("no changes added")
        | None -> false

    let private getNewCommand (_cmd: Command) =
        [ "git commit --amend --no-edit" ]

    let rule =
        Rule.create "git_commit_amend" matchCmd getNewCommand
        |> Rule.withPriority 1200
