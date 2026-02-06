namespace Dangit.Rules

open Dangit.Core

module GitPush =
    let private matchCmd (cmd: Command) =
        cmd.ScriptParts |> List.exists (fun p -> p = "push") &&
        cmd.Script.StartsWith("git ") &&
        match cmd.Output with
        | Some output -> output.Contains("git push --set-upstream")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.Output with
        | Some output ->
            output.Split('\n')
            |> Array.tryFind (fun line -> line.Trim().StartsWith("git push --set-upstream"))
            |> Option.map (fun line -> [line.Trim()])
            |> Option.defaultValue []
        | None -> []

    let rule =
        Rule.create "git_push" matchCmd getNewCommand
