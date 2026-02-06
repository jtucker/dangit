namespace Dangit.Core

module RuleEngine =
    /// Evaluate all rules against a command, returning sorted unique corrections
    let getCorrectedCommands (rules: Rule list) (command: Command) : CorrectedCommand list =
        rules
        |> List.filter (fun rule -> Rule.isMatch command rule)
        |> List.collect (fun rule -> Rule.getCorrectedCommands command rule)
        |> List.distinctBy (fun cmd -> cmd.Script)
        |> List.sortBy (fun cmd -> cmd.Priority)
