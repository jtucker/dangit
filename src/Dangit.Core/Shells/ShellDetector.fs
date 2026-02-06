namespace Dangit.Core.Shells

open System
open System.Runtime.InteropServices

module ShellDetector =
    /// Detect the current shell from environment
    let detect () : IShell =
        let shellEnv = Environment.GetEnvironmentVariable("SHELL")
        if not (String.IsNullOrEmpty(shellEnv)) then
            if shellEnv.Contains("zsh") then ZshShell() :> IShell
            elif shellEnv.Contains("bash") then BashShell() :> IShell
            else BashShell() :> IShell
        elif RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            PowerShellShell() :> IShell
        else
            BashShell() :> IShell
