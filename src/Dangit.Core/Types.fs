namespace Dangit.Core

/// Represents a command that was executed and potentially failed
type Command = {
    Script: string
    Output: string option
    ScriptParts: string list
}

module Command =
    /// Create a command from a script string and optional output
    let create (script: string) (output: string option) : Command =
        let parts =
            if System.String.IsNullOrWhiteSpace(script) then []
            else
                // Simple split — shells will override with proper parsing
                script.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
                |> Array.toList
        { Script = script; Output = output; ScriptParts = parts }

    /// Create a command with output
    let withOutput (script: string) (output: string) : Command =
        create script (Some output)

    /// Create a command without output
    let withoutOutput (script: string) : Command =
        create script None

/// Represents a corrected command suggested by a rule
type CorrectedCommand = {
    Script: string
    SideEffect: (Command -> string -> unit) option
    Priority: int
}

module CorrectedCommand =
    let create (script: string) (priority: int) : CorrectedCommand =
        { Script = script; SideEffect = None; Priority = priority }

    let withSideEffect (sideEffect: Command -> string -> unit) (cmd: CorrectedCommand) : CorrectedCommand =
        { cmd with SideEffect = Some sideEffect }

/// Represents a correction rule
type Rule = {
    Name: string
    Match: Command -> bool
    GetNewCommand: Command -> string list
    EnabledByDefault: bool
    SideEffect: (Command -> string -> unit) option
    Priority: int
    RequiresOutput: bool
}

module Rule =
    let create (name: string) (matchFn: Command -> bool) (getNewCommand: Command -> string list) : Rule =
        { Name = name
          Match = matchFn
          GetNewCommand = getNewCommand
          EnabledByDefault = true
          SideEffect = None
          Priority = 1000
          RequiresOutput = true }

    let withPriority (priority: int) (rule: Rule) : Rule =
        { rule with Priority = priority }

    let withEnabledByDefault (enabled: bool) (rule: Rule) : Rule =
        { rule with EnabledByDefault = enabled }

    let withRequiresOutput (requires: bool) (rule: Rule) : Rule =
        { rule with RequiresOutput = requires }

    /// Check if a rule matches a command
    let isMatch (command: Command) (rule: Rule) : bool =
        if rule.RequiresOutput && command.Output.IsNone then
            false
        else
            try rule.Match command
            with _ -> false

    /// Get corrected commands from a matching rule
    let getCorrectedCommands (command: Command) (rule: Rule) : CorrectedCommand list =
        let newCommands = rule.GetNewCommand command
        newCommands
        |> List.mapi (fun i cmd ->
            { Script = cmd
              SideEffect = rule.SideEffect
              Priority = (i + 1) * rule.Priority })
