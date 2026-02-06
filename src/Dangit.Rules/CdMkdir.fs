namespace Dangit.Rules

open Dangit.Core

module CdMkdir =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("cd ") &&
        match cmd.Output with
        | Some output ->
            let lower = output.ToLowerInvariant()
            lower.Contains("no such file or directory") ||
            lower.Contains("does not exist") ||
            lower.Contains("can't cd to") ||
            lower.Contains("the system cannot find the path")
        | None -> false

    let private getNewCommand (cmd: Command) =
        let dir = cmd.Script.Substring(3).Trim()
        [ sprintf "mkdir -p %s && cd %s" dir dir ]

    let rule =
        Rule.create "cd_mkdir" matchCmd getNewCommand
