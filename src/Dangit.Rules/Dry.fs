namespace Dangit.Rules

open Dangit.Core

module Dry =
    let private matchCmd (cmd: Command) =
        match cmd.ScriptParts with
        | first :: second :: _ when first = second -> true
        | _ -> false

    let private getNewCommand (cmd: Command) =
        match cmd.ScriptParts with
        | _ :: rest -> [ rest |> String.concat " " ]
        | _ -> []

    let rule =
        Rule.create "dry" matchCmd getNewCommand
        |> Rule.withRequiresOutput false
        |> Rule.withPriority 900
