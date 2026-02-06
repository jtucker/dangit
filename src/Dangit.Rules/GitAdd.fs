namespace Dangit.Rules

open Dangit.Core

module GitAdd =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git ") &&
        match cmd.Output with
        | Some output ->
            output.Contains("did not match any file(s) known to git") ||
            output.Contains("pathspec") && output.Contains("did not match")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.ScriptParts with
        | "git" :: "commit" :: _ ->
            [ "git add -A && " + cmd.Script ]
        | _ -> [ "git add ." ]

    let rule =
        Rule.create "git_add" matchCmd getNewCommand
