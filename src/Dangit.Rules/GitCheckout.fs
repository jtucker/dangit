namespace Dangit.Rules

open Dangit.Core

module GitCheckout =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git checkout ") &&
        match cmd.Output with
        | Some output ->
            output.Contains("did not match any") ||
            output.Contains("error: pathspec")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.Output with
        | Some output ->
            output.Split('\n')
            |> Array.choose (fun line ->
                let trimmed = line.Trim()
                if trimmed.Length > 0 && not (trimmed.StartsWith("error")) && not (trimmed.StartsWith("fatal")) then
                    Some trimmed
                else None)
            |> Array.truncate 3
            |> Array.map (fun suggestion -> sprintf "git checkout %s" suggestion)
            |> Array.toList
        | None -> []

    let rule =
        Rule.create "git_checkout" matchCmd getNewCommand
