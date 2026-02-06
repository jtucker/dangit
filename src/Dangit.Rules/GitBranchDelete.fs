namespace Dangit.Rules

open Dangit.Core

module GitBranchDelete =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git branch -") &&
        (cmd.Script.Contains("-d ") || cmd.Script.Contains("-D ")) &&
        match cmd.Output with
        | Some output ->
            output.Contains("checked out") ||
            output.Contains("Cannot delete branch")
        | None -> false

    let private getNewCommand (cmd: Command) =
        let parts = cmd.ScriptParts
        match parts |> List.tryLast with
        | Some branchName ->
            [ sprintf "git checkout main && git branch -D %s" branchName ]
        | None -> []

    let rule =
        Rule.create "git_branch_delete" matchCmd getNewCommand
