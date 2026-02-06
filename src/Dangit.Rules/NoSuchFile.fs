namespace Dangit.Rules

open System
open System.IO
open Dangit.Core

module NoSuchFile =
    let private matchCmd (cmd: Command) =
        match cmd.Output with
        | Some output ->
            let lower = output.ToLowerInvariant()
            lower.Contains("no such file or directory") ||
            lower.Contains("cannot find") ||
            lower.Contains("does not exist")
        | None -> false

    let private getNewCommand (cmd: Command) =
        match cmd.ScriptParts with
        | cmdName :: args when not args.IsEmpty ->
            let lastArg = args |> List.last
            let dir = Path.GetDirectoryName(lastArg)
            let fileName = Path.GetFileName(lastArg)
            let searchDir = if String.IsNullOrEmpty(dir) then "." else dir
            try
                if Directory.Exists(searchDir) then
                    Directory.GetFileSystemEntries(searchDir)
                    |> Array.map Path.GetFileName
                    |> Array.filter (fun f ->
                        f.ToLowerInvariant().Contains(fileName.ToLowerInvariant().Substring(0, max 1 (fileName.Length - 1))))
                    |> Array.truncate 3
                    |> Array.map (fun f ->
                        let correctedPath = if String.IsNullOrEmpty(dir) then f else Path.Combine(dir, f)
                        let correctedArgs = args |> List.map (fun a -> if a = lastArg then correctedPath else a)
                        sprintf "%s %s" cmdName (correctedArgs |> String.concat " "))
                    |> Array.toList
                else []
            with _ -> []
        | _ -> []

    let rule =
        Rule.create "no_such_file" matchCmd getNewCommand
