namespace Dangit.Rules

open Dangit.Core

module GitBranchExists =
    let private matchCmd (cmd: Command) =
        (cmd.Script.StartsWith("git branch ") || cmd.Script.StartsWith("git checkout -b ")) &&
        match cmd.Output with
        | Some output ->
            output.Contains("already exists") ||
            output.Contains("a branch named")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.ScriptParts with
        | "git" :: "checkout" :: "-b" :: branchName :: _ ->
            [ sprintf "git checkout %s" branchName ]
        | "git" :: "branch" :: branchName :: _ ->
            [ sprintf "git checkout %s" branchName ]
        | _ -> []

    let rule =
        Rule.create "git_branch_exists" matchCmd getNewCommand
