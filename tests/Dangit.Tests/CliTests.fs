module Dangit.Tests.CliTests

open Expecto
open Dangit.Core
open Dangit.Core.Shells
open Dangit.Cli.CliLogic

// We'll test the pure CLI logic functions that live in Dangit.Cli.CliLogic

/// Helper: validates that a Spectre markup string parses without throwing
let private validateMarkup (markup: string) =
    try
        Spectre.Console.Markup(markup) |> ignore
        true
    with
    | :? System.InvalidOperationException -> false

[<Tests>]
let markupValidationTests =
    testList "CLI.MarkupValidation" [
        test "all static markup strings are valid Spectre markup" {
            for markup in MarkupStrings.allStaticMarkup do
                Expect.isTrue (validateMarkup markup) $"Invalid markup: {markup}"
        }

        test "version template produces valid markup" {
            let markup = MarkupStrings.versionTemplate "0.1.0"
            Expect.isTrue (validateMarkup markup) "version markup should be valid"
        }

        test "correction template produces valid markup with simple text" {
            let markup = MarkupStrings.correctionTemplate "git push"
            Expect.isTrue (validateMarkup markup) "correction markup should be valid"
        }

        test "correction template escapes brackets in script" {
            let markup = MarkupStrings.correctionTemplate "echo [hello]"
            Expect.isTrue (validateMarkup markup) "correction markup should escape user brackets"
        }

        test "run template produces valid markup" {
            let markup = MarkupStrings.runTemplate "cd .."
            Expect.isTrue (validateMarkup markup) "run markup should be valid"
        }

        test "run template escapes brackets in script" {
            let markup = MarkupStrings.runTemplate "echo [world]"
            Expect.isTrue (validateMarkup markup) "run markup should escape user brackets"
        }

        test "selected template produces valid markup" {
            let markup = MarkupStrings.selectedTemplate "git pull --rebase"
            Expect.isTrue (validateMarkup markup) "selected markup should be valid"
        }

        test "selected template escapes special chars in script" {
            let markup = MarkupStrings.selectedTemplate "echo [red]not-a-tag[/]"
            Expect.isTrue (validateMarkup markup) "selected markup should escape spectre-like user input"
        }

        test "version template with unusual version string is valid" {
            let markup = MarkupStrings.versionTemplate "10.0.200-preview.1"
            Expect.isTrue (validateMarkup markup) "version with preview suffix should be valid"
        }
    ]

[<Tests>]
let aliasTests =
    testList "CLI.Alias" [
        test "generateAliasScript for bash returns bash function" {
            let shell = BashShell() :> IShell
            let script = Dangit.Cli.CliLogic.generateAliasScript shell "dangit"
            Expect.stringContains script "function dangit" "Should contain bash function"
            Expect.stringContains script "dangit" "Should reference dangit"
        }

        test "generateAliasScript for powershell returns PS function" {
            let shell = PowerShellShell() :> IShell
            let script = Dangit.Cli.CliLogic.generateAliasScript shell "dang"
            Expect.stringContains script "function dang" "Should contain PS function with custom name"
        }

        test "generateAliasScript for zsh returns zsh function" {
            let shell = ZshShell() :> IShell
            let script = Dangit.Cli.CliLogic.generateAliasScript shell "oops"
            Expect.stringContains script "function oops" "Should contain zsh function with custom name"
        }

        test "generateAliasScript uses settings alias when no name given" {
            let shell = BashShell() :> IShell
            let settings = { Configuration.defaultSettings with AliasName = "fix" }
            let script = Dangit.Cli.CliLogic.generateAliasScriptFromSettings shell settings None
            Expect.stringContains script "function fix" "Should use settings alias name"
        }

        test "generateAliasScript uses explicit name over settings" {
            let shell = BashShell() :> IShell
            let settings = { Configuration.defaultSettings with AliasName = "fix" }
            let script = Dangit.Cli.CliLogic.generateAliasScriptFromSettings shell settings (Some "oops")
            Expect.stringContains script "function oops" "Explicit name takes priority"
        }
    ]

[<Tests>]
let allRulesTests =
    testList "CLI.AllRules" [
        test "allRules returns 21 rules" {
            let rules = Dangit.Cli.CliLogic.allRules
            Expect.equal (List.length rules) 21 "Should have 21 rules"
        }

        test "allRules contains cd_parent" {
            let rules = Dangit.Cli.CliLogic.allRules
            let names = rules |> List.map (fun r -> r.Name)
            Expect.contains names "cd_parent" "Should contain cd_parent"
        }

        test "allRules contains git_push" {
            let rules = Dangit.Cli.CliLogic.allRules
            let names = rules |> List.map (fun r -> r.Name)
            Expect.contains names "git_push" "Should contain git_push"
        }

        test "allRules all have unique names" {
            let rules = Dangit.Cli.CliLogic.allRules
            let names = rules |> List.map (fun r -> r.Name)
            let unique = names |> List.distinct
            Expect.equal (List.length unique) (List.length names) "All rule names should be unique"
        }
    ]

[<Tests>]
let ruleListingTests =
    testList "CLI.RuleListing" [
        test "getRuleListItems returns all rules with enabled status" {
            let settings = Configuration.defaultSettings
            let items = Dangit.Cli.CliLogic.getRuleListItems settings
            Expect.equal (List.length items) 21 "Should return 21 rule items"
        }

        test "getRuleListItems shows enabled for default rules" {
            let settings = Configuration.defaultSettings
            let items = Dangit.Cli.CliLogic.getRuleListItems settings
            let cdParent = items |> List.find (fun (name, _, _) -> name = "cd_parent")
            let (_, enabled, _) = cdParent
            Expect.isTrue enabled "cd_parent should be enabled by default"
        }

        test "getRuleListItems shows disabled for excluded rules" {
            let settings = { Configuration.defaultSettings with ExcludeRules = ["cd_parent"] }
            let items = Dangit.Cli.CliLogic.getRuleListItems settings
            let cdParent = items |> List.find (fun (name, _, _) -> name = "cd_parent")
            let (_, enabled, _) = cdParent
            Expect.isFalse enabled "cd_parent should be disabled when excluded"
        }

        test "getRuleListItems returns priority" {
            let settings = Configuration.defaultSettings
            let items = Dangit.Cli.CliLogic.getRuleListItems settings
            let sudo = items |> List.find (fun (name, _, _) -> name = "sudo")
            let (_, _, priority) = sudo
            Expect.equal priority 900 "sudo should have priority 900"
        }
    ]

[<Tests>]
let correctionFlowTests =
    testList "CLI.CorrectionFlow" [
        test "getEnabledRules filters by settings" {
            let settings = { Configuration.defaultSettings with ExcludeRules = ["cd_parent"; "sudo"] }
            let enabled = Dangit.Cli.CliLogic.getEnabledRules settings
            let names = enabled |> List.map (fun r -> r.Name)
            Expect.isFalse (names |> List.contains "cd_parent") "cd_parent should be excluded"
            Expect.isFalse (names |> List.contains "sudo") "sudo should be excluded"
            Expect.equal (List.length enabled) 19 "Should have 19 rules after excluding 2"
        }

        test "findCorrections returns corrections for matching command" {
            let cmd = Command.create "cd.." None
            let settings = Configuration.defaultSettings
            let corrections = Dangit.Cli.CliLogic.findCorrections settings cmd
            Expect.isNonEmpty corrections "Should find corrections for cd.."
            Expect.equal corrections.[0].Script "cd .." "Should correct to 'cd ..'"
        }

        test "findCorrections returns empty for unrecognized command" {
            let cmd = Command.create "some_random_thing_xyz" (Some "all good")
            let settings = Configuration.defaultSettings
            let corrections = Dangit.Cli.CliLogic.findCorrections settings cmd
            Expect.isEmpty corrections "Should find no corrections"
        }
    ]

[<Tests>]
let configDisplayTests =
    testList "CLI.ConfigDisplay" [
        test "formatConfigItems returns key-value pairs" {
            let settings = Configuration.defaultSettings
            let items = Dangit.Cli.CliLogic.formatConfigItems settings
            Expect.isNonEmpty items "Should return config items"
            let keys = items |> List.map fst
            Expect.contains keys "Alias" "Should contain Alias key"
            Expect.contains keys "Rules" "Should contain Rules key"
            Expect.contains keys "Wait Command" "Should contain Wait Command"
            Expect.contains keys "Debug Mode" "Should contain Debug Mode"
        }

        test "formatConfigItems shows correct values" {
            let settings = { Configuration.defaultSettings with AliasName = "fix"; DebugMode = true }
            let items = Dangit.Cli.CliLogic.formatConfigItems settings
            let alias = items |> List.find (fun (k, _) -> k = "Alias") |> snd
            Expect.equal alias "fix" "Should show alias value"
            let debug = items |> List.find (fun (k, _) -> k = "Debug Mode") |> snd
            Expect.equal debug "True" "Should show debug as true"
        }
    ]

[<Tests>]
let parseArgsTests =
    testList "CLI.ParseArgs" [
        test "empty args returns RunCorrection mode" {
            let mode = Dangit.Cli.CliLogic.parseArgs [||]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.RunCorrection(None, None)) "Empty args = RunCorrection"
        }

        test "version flag returns ShowVersion" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--version"|]
            Expect.equal mode Dangit.Cli.CliLogic.CliMode.ShowVersion "Should be ShowVersion"
        }

        test "rules subcommand returns ListRules" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"rules"|]
            Expect.equal mode Dangit.Cli.CliLogic.CliMode.ListRules "Should be ListRules"
        }

        test "config subcommand returns ShowConfig" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"config"|]
            Expect.equal mode Dangit.Cli.CliLogic.CliMode.ShowConfig "Should be ShowConfig"
        }

        test "alias subcommand returns ShowAlias with no name" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"alias"|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.ShowAlias None) "Should be ShowAlias None"
        }

        test "alias with name returns ShowAlias with name" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"alias"; "oops"|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.ShowAlias (Some "oops")) "Should be ShowAlias Some oops"
        }

        test "--alias flag returns ShowAlias with no name" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--alias"|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.ShowAlias None) "Should be ShowAlias None"
        }

        test "--alias with name returns ShowAlias with name" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--alias"; "fix"|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.ShowAlias (Some "fix")) "Should be ShowAlias Some fix"
        }

        test "--command and --output pass through to RunCorrection" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--command"; "git push"; "--output"; "error msg"|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.RunCorrection(Some "git push", Some "error msg")) "Should pass command and output"
        }

        test "--command without --output works" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--command"; "cd.."|]
            Expect.equal mode (Dangit.Cli.CliLogic.CliMode.RunCorrection(Some "cd..", None)) "Should pass command only"
        }

        test "--help returns ShowHelp" {
            let mode = Dangit.Cli.CliLogic.parseArgs [|"--help"|]
            Expect.equal mode Dangit.Cli.CliLogic.CliMode.ShowHelp "Should be ShowHelp"
        }
    ]
