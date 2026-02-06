namespace Dangit.Rules

open Dangit.Core

module GitUnknownCommand =
    let private gitSubcommands = [
        "add"; "bisect"; "branch"; "checkout"; "cherry-pick"; "clean"; "clone"
        "commit"; "config"; "diff"; "fetch"; "grep"; "init"; "log"; "merge"
        "mv"; "pull"; "push"; "rebase"; "reflog"; "remote"; "reset"; "restore"
        "revert"; "rm"; "show"; "stash"; "status"; "switch"; "tag"; "worktree"
    ]

    let private levenshtein (s1: string) (s2: string) : int =
        let len1, len2 = s1.Length, s2.Length
        let d = Array2D.init (len1 + 1) (len2 + 1) (fun i j ->
            if i = 0 then j elif j = 0 then i else 0)
        for i in 1..len1 do
            for j in 1..len2 do
                let cost = if s1.[i-1] = s2.[j-1] then 0 else 1
                d.[i,j] <- min (min (d.[i-1,j] + 1) (d.[i,j-1] + 1)) (d.[i-1,j-1] + cost)
        d.[len1, len2]

    let private parseSuggestionsFromOutput (output: string) =
        let lines = output.Split('\n')
        // Git outputs suggestions after "The most similar command is" or "The most similar commands are"
        let mutable foundSuggestionHeader = false
        [ for line in lines do
            let trimmed = line.Trim()
            if trimmed.Contains("The most similar command") then
                foundSuggestionHeader <- true
            elif foundSuggestionHeader && trimmed.Length > 0 then
                yield trimmed ]

    let private matchCmd (cmd: Command) =
        cmd.Script.StartsWith("git ") &&
        match cmd.Output with
        | Some output -> output.Contains("is not a git command")
        | None -> false

    let private getNewCommand (cmd: Command) =
        let trailingArgs =
            match cmd.ScriptParts with
            | _ :: _ :: rest -> rest
            | _ -> []
        let argSuffix =
            if trailingArgs.IsEmpty then ""
            else " " + (trailingArgs |> String.concat " ")

        // First try: extract suggestions from git's own output
        let gitSuggestions =
            match cmd.Output with
            | Some output -> parseSuggestionsFromOutput output
            | None -> []

        if not gitSuggestions.IsEmpty then
            gitSuggestions |> List.map (fun s -> "git " + s + argSuffix)
        else
            // Fallback: fuzzy match against known subcommands
            match cmd.ScriptParts with
            | _ :: typo :: _ ->
                gitSubcommands
                |> List.map (fun c -> (c, levenshtein (typo.ToLowerInvariant()) c))
                |> List.filter (fun (_, d) -> d <= 2 && d > 0)
                |> List.sortBy snd
                |> List.truncate 3
                |> List.map (fun (suggestion, _) -> "git " + suggestion + argSuffix)
            | _ -> []

    let rule =
        Rule.create "git_unknown_command" matchCmd getNewCommand
        |> Rule.withPriority 900
