namespace Dangit.Core

open System
open System.IO
open System.Text.Json

type DangitSettings = {
    Rules: string list
    ExcludeRules: string list
    AliasName: string
    AlterHistory: bool
    WaitCommand: int
    DebugMode: bool
}

module Configuration =
    let defaultSettings : DangitSettings = {
        Rules = [ "ALL" ]
        ExcludeRules = []
        AliasName = "dangit"
        AlterHistory = true
        WaitCommand = 3
        DebugMode = false
    }

    let private configDir () =
        let home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        Path.Combine(home, ".config", "dangit")

    let private configPath () =
        Path.Combine(configDir (), "settings.json")

    let private jsonOptions =
        let opts = JsonSerializerOptions()
        opts.WriteIndented <- true
        opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        opts

    /// Load settings from config file, falling back to defaults
    let load () : DangitSettings =
        let path = configPath ()
        if File.Exists(path) then
            try
                let json = File.ReadAllText(path)
                JsonSerializer.Deserialize<DangitSettings>(json, jsonOptions)
            with _ ->
                defaultSettings
        else
            defaultSettings

    /// Save settings to config file
    let save (settings: DangitSettings) : unit =
        let dir = configDir ()
        if not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore
        let json = JsonSerializer.Serialize(settings, jsonOptions)
        File.WriteAllText(configPath (), json)

    /// Check if a rule is enabled by the settings
    let isRuleEnabled (settings: DangitSettings) (rule: Rule) : bool =
        let isExcluded = settings.ExcludeRules |> List.contains rule.Name
        if isExcluded then false
        elif settings.Rules |> List.contains "ALL" then rule.EnabledByDefault
        else settings.Rules |> List.contains rule.Name
