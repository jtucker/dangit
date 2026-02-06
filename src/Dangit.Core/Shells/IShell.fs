namespace Dangit.Core.Shells

open Dangit.Core

/// Shell abstraction for cross-platform command execution
type IShell =
    /// Get the name of the shell (e.g., "powershell", "bash", "zsh")
    abstract member Name: string
    /// Get the last executed command and its output from shell history
    abstract member GetLastCommand: unit -> Command option
    /// Split a command string into parts (shell-specific parsing)
    abstract member SplitCommand: string -> string list
    /// Quote a string for safe shell usage
    abstract member Quote: string -> string
    /// Generate the alias setup script for this shell
    abstract member GenerateAlias: aliasName: string -> string
    /// Compose two commands with AND (&&)
    abstract member AndCommands: string -> string -> string
    /// Compose two commands with OR (||)
    abstract member OrCommands: string -> string -> string
    /// Put a command into shell history
    abstract member PutToHistory: string -> unit
