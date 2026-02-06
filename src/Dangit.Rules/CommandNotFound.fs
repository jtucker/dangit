namespace Dangit.Rules

open Dangit.Core

module CommandNotFound =
    /// Simple Levenshtein distance
    let internal levenshtein (s1: string) (s2: string) : int =
        let len1, len2 = s1.Length, s2.Length
        let d = Array2D.init (len1 + 1) (len2 + 1) (fun i j ->
            if i = 0 then j elif j = 0 then i else 0)
        for i in 1..len1 do
            for j in 1..len2 do
                let cost = if s1.[i-1] = s2.[j-1] then 0 else 1
                d.[i,j] <- min (min (d.[i-1,j] + 1) (d.[i,j-1] + 1)) (d.[i-1,j-1] + cost)
        d.[len1, len2]

    let private commonCommands = [
        "ls"; "cd"; "cat"; "rm"; "cp"; "mv"; "mkdir"; "rmdir"; "touch"; "chmod"
        "grep"; "find"; "head"; "tail"; "less"; "more"; "wc"; "sort"; "uniq"
        "git"; "docker"; "npm"; "node"; "python"; "pip"; "dotnet"; "cargo"
        "curl"; "wget"; "ssh"; "scp"; "tar"; "zip"; "unzip"
        "echo"; "printf"; "which"; "where"; "man"; "help"
        "ps"; "kill"; "top"; "htop"; "df"; "du"; "free"
    ]

    let private matchCmd (cmd: Command) =
        match cmd.Output with
        | Some output ->
            let lower = output.ToLowerInvariant()
            lower.Contains("command not found") ||
            lower.Contains("not recognized") ||
            lower.Contains("is not recognized as") ||
            lower.Contains("unknown command")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.ScriptParts with
        | cmdName :: args ->
            commonCommands
            |> List.map (fun c -> (c, levenshtein (cmdName.ToLowerInvariant()) c))
            |> List.filter (fun (_, d) -> d <= 2 && d > 0)
            |> List.sortBy snd
            |> List.truncate 3
            |> List.map (fun (suggestion, _) ->
                if args.IsEmpty then suggestion
                else sprintf "%s %s" suggestion (args |> String.concat " "))
        | _ -> []

    let rule =
        Rule.create "command_not_found" matchCmd getNewCommand
        |> Rule.withPriority 800
