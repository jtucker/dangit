namespace Dangit.Rules

open Dangit.Core

module SudoCommand =
    let private matchCmd (cmd: Command) =
        not (cmd.Script.StartsWith("sudo ")) &&
        match cmd.Output with
        | Some output ->
            let lower = output.ToLowerInvariant()
            lower.Contains("permission denied") ||
            lower.Contains("access denied") ||
            lower.Contains("operation not permitted") ||
            lower.Contains("must be run as root") ||
            lower.Contains("requires root") ||
            lower.Contains("access is denied")
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ sprintf "sudo %s" cmd.Script ]

    let rule =
        Rule.create "sudo" matchCmd getNewCommand
        |> Rule.withPriority 900
