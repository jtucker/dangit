namespace Dangit.Rules

open Dangit.Core

module CdParent =
    let private matchCmd (cmd: Command) =
        cmd.Script = "cd.." || cmd.Script.StartsWith("cd..")

    let private getNewCommand (cmd: Command) =
        [ cmd.Script.Replace("cd..", "cd ..") ]

    let rule =
        Rule.create "cd_parent" matchCmd getNewCommand
        |> Rule.withRequiresOutput false
