namespace Dangit.Rules

open Dangit.Core

module GitDiffStaged =
    let private matchCmd (cmd: Command) =
        cmd.Script = "git diff" &&
        match cmd.Output with
        | Some output -> output.Trim() = "" || output.Trim().Length = 0
        | None -> true

    let private getNewCommand (_cmd: Command) =
        [ "git diff --staged" ]

    let rule =
        Rule.create "git_diff_staged" matchCmd getNewCommand
        |> Rule.withRequiresOutput false
        |> Rule.withPriority 1500
