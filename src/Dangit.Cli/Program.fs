module Dangit.Cli.Program

open System
open System.Reflection
open Spectre.Console
open Dangit.Core
open Dangit.Core.Shells
open Dangit.Cli.CliLogic
open Dangit.Telemetry

let private version () =
    let asm = Assembly.GetExecutingAssembly()
    let ver = asm.GetName().Version
    if isNull ver then "0.1.0" else ver.ToString(3)

let private showVersion () =
    AnsiConsole.MarkupLine(MarkupStrings.versionTemplate (version ()))
    0

let private showHelp () =
    AnsiConsole.MarkupLine(MarkupStrings.helpTitle)
    AnsiConsole.WriteLine()
    AnsiConsole.MarkupLine(MarkupStrings.helpUsageHeader)
    AnsiConsole.WriteLine("  dangit                         Run correction flow")
    AnsiConsole.WriteLine("  dangit --command CMD [--output OUT]  Correct a specific command")
    AnsiConsole.WriteLine("  dangit alias [name]            Print shell alias setup script")
    AnsiConsole.WriteLine("  dangit --alias [name]          Print shell alias setup script")
    AnsiConsole.WriteLine("  dangit rules                   List available rules")
    AnsiConsole.WriteLine("  dangit config                  Show current configuration")
    AnsiConsole.WriteLine("  dangit --version               Show version info")
    AnsiConsole.WriteLine("  dangit --help                  Show this help")
    0

let private showAlias (shell: IShell) (settings: DangitSettings) (name: string option) =
    let script = generateAliasScriptFromSettings shell settings name
    AnsiConsole.WriteLine(script)
    0

let private listRules (settings: DangitSettings) =
    let table = Table()
    table.Border <- TableBorder.Rounded
    table.Title <- TableTitle(MarkupStrings.rulesTableTitle)
    table.AddColumn(TableColumn(MarkupStrings.ruleColumnHeader)) |> ignore
    table.AddColumn(TableColumn(MarkupStrings.statusColumnHeader)) |> ignore
    table.AddColumn(TableColumn(MarkupStrings.priorityColumnHeader)) |> ignore

    let items = getRuleListItems settings
    for (name, enabled, priority) in items do
        let status =
            if enabled then MarkupStrings.enabledStatus
            else MarkupStrings.disabledStatus
        table.AddRow(Markup(name), Markup(status), Markup(string priority)) |> ignore

    AnsiConsole.Write(table)
    0

let private showConfig (settings: DangitSettings) =
    let table = Table()
    table.Border <- TableBorder.Rounded
    table.Title <- TableTitle(MarkupStrings.configTableTitle)
    table.AddColumn(TableColumn(MarkupStrings.settingColumnHeader)) |> ignore
    table.AddColumn(TableColumn(MarkupStrings.valueColumnHeader)) |> ignore

    let items = formatConfigItems settings
    for (key, value) in items do
        let displayValue = if String.IsNullOrEmpty(value) then MarkupStrings.noneValue else Markup.Escape(value)
        table.AddRow(Markup(key), Markup(displayValue)) |> ignore

    AnsiConsole.Write(table)
    0

let private displayCorrections (corrections: CorrectedCommand list) (command: Command) (shell: IShell) (settings: DangitSettings) =
    match corrections with
    | [] ->
        AnsiConsole.MarkupLine(MarkupStrings.noCorrections)
        1
    | [ single ] ->
        AnsiConsole.MarkupLine(MarkupStrings.correctionTemplate single.Script)
        single.SideEffect |> Option.iter (fun se -> se command single.Script)
        if settings.AlterHistory then shell.PutToHistory(single.Script)
        AnsiConsole.MarkupLine(MarkupStrings.runTemplate single.Script)
        0
    | multiple ->
        let prompt =
            SelectionPrompt<string>()
                .Title(MarkupStrings.multipleCorrectionsTitle)
                .PageSize(10)
        for c in multiple do
            prompt.AddChoice(c.Script) |> ignore

        let selected = AnsiConsole.Prompt(prompt)
        AnsiConsole.MarkupLine(MarkupStrings.selectedTemplate selected)

        let correction = multiple |> List.find (fun c -> c.Script = selected)
        correction.SideEffect |> Option.iter (fun se -> se command selected)
        if settings.AlterHistory then shell.PutToHistory(selected)
        0

let private runCorrection (shell: IShell) (settings: DangitSettings) (cmdStr: string option) (outputStr: string option) =
    let command =
        match cmdStr with
        | Some c -> Some (Command.create c outputStr)
        | None ->
            match shell.GetLastCommand() with
            | Some cmd -> Some cmd
            | None -> None

    match command with
    | None ->
        AnsiConsole.MarkupLine(MarkupStrings.noCommandFound)
        1
    | Some cmd ->
        let corrections =
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start(MarkupStrings.evaluatingRules, fun _ ->
                    DangitTelemetry.timed "findCorrections" (fun () ->
                        let result = findCorrections settings cmd
                        DangitTelemetry.recordRulesEvaluated (List.length (getEnabledRules settings))
                        DangitTelemetry.recordRulesMatched (List.length result)
                        result))

        displayCorrections corrections cmd shell settings

[<EntryPoint>]
let main argv =
    let settings = Configuration.load ()

    // Initialize telemetry (opt-in via DANGIT_TELEMETRY env var)
    let telemetryEnabled =
        match Environment.GetEnvironmentVariable("DANGIT_TELEMETRY") with
        | null | "" | "0" | "false" -> false
        | _ -> true
    let connStr =
        match Environment.GetEnvironmentVariable("DANGIT_TELEMETRY_CONNECTION") with
        | null | "" -> None
        | s -> Some s
    TelemetrySetup.initialize (TelemetryConfig.create telemetryEnabled connStr)

    let shell = ShellDetector.detect ()

    let exitCode =
        match parseArgs argv with
        | CliMode.ShowVersion -> showVersion ()
        | CliMode.ShowHelp -> showHelp ()
        | CliMode.ShowAlias name -> showAlias shell settings name
        | CliMode.ListRules -> listRules settings
        | CliMode.ShowConfig -> showConfig settings
        | CliMode.RunCorrection(cmd, output) -> runCorrection shell settings cmd output

    TelemetrySetup.shutdown ()
    exitCode
