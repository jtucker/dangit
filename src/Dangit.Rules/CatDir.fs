namespace Dangit.Rules

open Dangit.Core

module CatDir =
    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("cat ") &&
        match cmd.Output with
        | Some output ->
            output.ToLowerInvariant().Contains("is a directory")
        | None -> false

    let private getNewCommand (cmd: Command) =
        [ cmd.Script.Replace("cat ", "ls ") ]

    let rule =
        Rule.create "cat_dir" matchCmd getNewCommand
