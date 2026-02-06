namespace Dangit.Rules

open Dangit.Core

module GitRebaseNoChanges =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git rebase") &&
        match cmd.Output with
        | Some output ->
            output.Contains("No changes") ||
            output.Contains("nothing to commit") ||
            output.Contains("Could not apply")
        | None -> false

    let private getNewCommand (_cmd: Command) =
        [ "git rebase --skip" ]

    let rule =
        Rule.create "git_rebase_no_changes" matchCmd getNewCommand
