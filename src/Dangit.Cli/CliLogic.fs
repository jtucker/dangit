namespace Dangit.Cli

open Dangit.Core
open Dangit.Core.Shells

module CliLogic =

    // --- CLI Mode discriminated union ---

    [<Struct>]
    type CliMode =
        | RunCorrection of command: string option * output: string option
        | ShowVersion
        | ShowHelp
        | ShowAlias of aliasName: string option
        | ListRules
        | ShowConfig

    // --- All 20 rules from Dangit.Rules ---

    let allRules : Rule list =
        [ Dangit.Rules.CdMkdir.rule
          Dangit.Rules.CdParent.rule
          Dangit.Rules.Dry.rule
          Dangit.Rules.CatDir.rule
          Dangit.Rules.RmDir.rule
          Dangit.Rules.MkdirP.rule
          Dangit.Rules.CommandNotFound.rule
          Dangit.Rules.NoSuchFile.rule
          Dangit.Rules.SudoCommand.rule
          Dangit.Rules.FixFile.rule
          Dangit.Rules.GitPush.rule
          Dangit.Rules.GitAdd.rule
          Dangit.Rules.GitBranchDelete.rule
          Dangit.Rules.GitBranchExists.rule
          Dangit.Rules.GitCheckout.rule
          Dangit.Rules.GitDiffStaged.rule
          Dangit.Rules.GitPull.rule
          Dangit.Rules.GitStash.rule
          Dangit.Rules.GitCommitAmend.rule
          Dangit.Rules.GitRebaseNoChanges.rule
          Dangit.Rules.GitUnknownCommand.rule ]

    // --- Alias generation ---

    let generateAliasScript (shell: IShell) (aliasName: string) : string =
        shell.GenerateAlias(aliasName)

    let generateAliasScriptFromSettings (shell: IShell) (settings: DangitSettings) (explicitName: string option) : string =
        let name = explicitName |> Option.defaultValue settings.AliasName
        generateAliasScript shell name

    // --- Rule listing ---

    /// Returns (name, enabled, priority) tuples for all rules
    let getRuleListItems (settings: DangitSettings) : (string * bool * int) list =
        allRules
        |> List.map (fun rule ->
            (rule.Name, Configuration.isRuleEnabled settings rule, rule.Priority))

    // --- Correction flow ---

    let getEnabledRules (settings: DangitSettings) : Rule list =
        allRules |> List.filter (Configuration.isRuleEnabled settings)

    let findCorrections (settings: DangitSettings) (command: Command) : CorrectedCommand list =
        let enabledRules = getEnabledRules settings
        RuleEngine.getCorrectedCommands enabledRules command

    // --- Config display ---

    let formatConfigItems (settings: DangitSettings) : (string * string) list =
        [ "Alias", settings.AliasName
          "Rules", (settings.Rules |> String.concat ", ")
          "Exclude Rules", (settings.ExcludeRules |> String.concat ", ")
          "Alter History", (string settings.AlterHistory)
          "Wait Command", (string settings.WaitCommand)
          "Debug Mode", (string settings.DebugMode) ]

    // --- Markup strings (centralized for testability) ---

    module MarkupStrings =
        let helpTitle = "[bold green]dangit[/] — command corrector"
        let helpUsageHeader = "[yellow]Usage:[/]"
        let versionTemplate version = $"[bold green]dangit[/] version [cyan]{version}[/]"
        let noCorrections = "[red]No corrections found.[/]"
        let correctionTemplate script = $"[bold green]Correction:[/] [cyan]{Spectre.Console.Markup.Escape(script)}[/]"
        let runTemplate script = $"[dim]Run:[/] {Spectre.Console.Markup.Escape(script)}"
        let selectedTemplate script = $"[bold green]Selected:[/] [cyan]{Spectre.Console.Markup.Escape(script)}[/]"
        let multipleCorrectionsTitle = "[bold green]Multiple corrections found — select one:[/]"
        let noCommandFound = "[red]No failed command found.[/] Pass a command with [yellow]--command[/] or set up the shell alias."
        let evaluatingRules = "[yellow]Evaluating rules...[/]"
        let rulesTableTitle = "[bold green]dangit[/] rules"
        let ruleColumnHeader = "[bold]Rule[/]"
        let statusColumnHeader = "[bold]Status[/]"
        let priorityColumnHeader = "[bold]Priority[/]"
        let enabledStatus = "[green]enabled[/]"
        let disabledStatus = "[dim]disabled[/]"
        let configTableTitle = "[bold green]dangit[/] configuration"
        let settingColumnHeader = "[bold]Setting[/]"
        let valueColumnHeader = "[bold]Value[/]"
        let noneValue = "[dim](none)[/]"

        /// All static markup strings used in the CLI
        let allStaticMarkup =
            [ helpTitle
              helpUsageHeader
              noCorrections
              multipleCorrectionsTitle
              noCommandFound
              evaluatingRules
              rulesTableTitle
              ruleColumnHeader
              statusColumnHeader
              priorityColumnHeader
              enabledStatus
              disabledStatus
              configTableTitle
              settingColumnHeader
              valueColumnHeader
              noneValue ]

    // --- Argument parsing ---

    let parseArgs (argv: string array) : CliMode =
        let args = argv |> Array.toList
        match args with
        | [] -> RunCorrection(None, None)
        | ["--version"] -> ShowVersion
        | ["--help"] -> ShowHelp
        | ["--alias"] -> ShowAlias None
        | ["--alias"; name] -> ShowAlias(Some name)
        | ["alias"] -> ShowAlias None
        | ["alias"; name] -> ShowAlias(Some name)
        | ["rules"] -> ListRules
        | ["config"] -> ShowConfig
        | _ ->
            // Parse --command and --output flags
            let rec parse cmd out remaining =
                match remaining with
                | "--command" :: value :: rest -> parse (Some value) out rest
                | "--output" :: value :: rest -> parse cmd (Some value) rest
                | _ :: rest -> parse cmd out rest
                | [] -> RunCorrection(cmd, out)
            parse None None args
