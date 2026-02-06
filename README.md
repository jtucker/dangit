# 🔧 dangit

**A cross-platform command corrector for your terminal.**

[![Build](https://github.com/jtucker/dangit/actions/workflows/ci.yml/badge.svg)](https://github.com/jtucker/dangit/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Release](https://img.shields.io/github/v/release/jtucker/dangit)](https://github.com/jtucker/dangit/releases)

Written in F# 10 / .NET 10. Inspired by [thefuck](https://github.com/nvbn/thefuck) — but ships as a single native executable with zero dependencies.

---

## What It Does

When your previous command fails, type `dangit` and it suggests the corrected command.

**Before:**

```console
$ git pussh origin main
git: 'pussh' is not a git command. See 'git --help'.
```

**After:**

```console
$ dangit
✔ Correction: git push origin main
```

It evaluates 20 built-in rules (10 core + 10 git-specific), picks the best correction, and presents it with a rich terminal UI powered by [Spectre.Console](https://spectreconsole.net/). When multiple corrections match, you get an interactive selection prompt.

---

## Installation

### winget (Windows)

```powershell
winget install codingforbeer.dangit
```

### GitHub Releases

Download the single executable for your platform from [Releases](https://github.com/jtucker/dangit/releases). No runtime required — it's self-contained.

| Platform      | Binary             |
| ------------- | ------------------ |
| Windows x64   | `dangit-win-x64`   |
| Windows ARM64 | `dangit-win-arm64` |
| Linux x64     | `dangit-linux-x64` |
| Linux ARM64   | `dangit-linux-arm64` |
| macOS x64     | `dangit-osx-x64`   |
| macOS ARM64   | `dangit-osx-arm64` |

### From Source

```bash
git clone https://github.com/jtucker/dangit.git
cd dangit
dotnet build
```

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

---

## Quick Start — Shell Setup

For `dangit` to automatically capture your last failed command, add a shell alias. Run `dangit --alias` to print the setup script for your detected shell, or configure manually:

### PowerShell

Add to your `$PROFILE`:

```powershell
function dangit {
    $lastCmd = (Get-History -Count 1).CommandLine
    $lastOutput = $Error[0] | Out-String
    dangit --command $lastCmd --output $lastOutput
}
```

Or run:

```powershell
dangit --alias >> $PROFILE
```

### Bash

Add to `~/.bashrc`:

```bash
function dangit () {
    local last_cmd=$(fc -ln -1)
    local last_output=$(eval "$last_cmd" 2>&1)
    dangit --command "$last_cmd" --output "$last_output"
}
alias dangit='dangit'
```

Or run:

```bash
dangit --alias >> ~/.bashrc
source ~/.bashrc
```

### Zsh

Add to `~/.zshrc`:

```zsh
function dangit () {
    local last_cmd=$(fc -ln -1)
    local last_output=$(eval "$last_cmd" 2>&1)
    dangit --command "$last_cmd" --output "$last_output"
}
alias dangit='dangit'
```

Or run:

```bash
dangit --alias >> ~/.zshrc
source ~/.zshrc
```

### Custom Alias Name

You can change the alias name from the default `dangit`:

```bash
dangit --alias fix        # prints alias script using "fix" as the name
dangit alias oops         # same thing, without the --
```

---

## Usage

```bash
# Auto-detect last failed command (requires shell alias)
dangit

# Correct a specific command
dangit --command "git pussh" --output "git: 'pussh' is not a git command."

# List all available rules and their status
dangit rules

# Show current configuration
dangit config

# Print shell alias script
dangit --alias

# Show version
dangit --version

# Show help
dangit --help
```

---

## Available Rules

### Core Rules (10)

| Rule | Description | Example |
| ---- | ----------- | ------- |
| `cd_mkdir` | Creates missing directory then `cd`s into it | `cd newdir` → `mkdir -p newdir && cd newdir` |
| `cd_parent` | Fixes missing space in `cd..` | `cd..` → `cd ..` |
| `dry` | Removes accidentally duplicated command | `git git status` → `git status` |
| `cat_dir` | Replaces `cat` with `ls` when target is a directory | `cat mydir` → `ls mydir` |
| `rm_dir` | Adds `-rf` flag when `rm` fails on a directory | `rm mydir` → `rm -rf mydir` |
| `mkdir_p` | Adds `-p` flag for nested directory creation | `mkdir a/b/c` → `mkdir -p a/b/c` |
| `command_not_found` | Suggests closest command using Levenshtein distance | `gti status` → `git status` |
| `no_such_file` | Suggests similar filenames in the directory | `cat myfile.tx` → `cat myfile.txt` |
| `sudo` | Prepends `sudo` on permission denied errors | `apt install foo` → `sudo apt install foo` |
| `fix_file` | Adds execute permission and retries | `./script.sh` → `chmod +x ./script.sh && ./script.sh` |

### Git Rules (10)

| Rule | Description | Example |
| ---- | ----------- | ------- |
| `git_push` | Sets upstream branch on first push | `git push` → `git push --set-upstream origin feature` |
| `git_add` | Stages files when commit fails due to unstaged changes | `git commit` → `git add -A && git commit` |
| `git_branch_delete` | Checks out main first when deleting current branch | `git branch -d feat` → `git checkout main && git branch -D feat` |
| `git_branch_exists` | Checks out existing branch instead of creating it | `git checkout -b main` → `git checkout main` |
| `git_checkout` | Suggests similar branch names on pathspec error | `git checkout faeture` → `git checkout feature` |
| `git_diff_staged` | Adds `--staged` when `git diff` shows nothing | `git diff` → `git diff --staged` |
| `git_pull` | Pulls before pushing on non-fast-forward rejection | `git push` → `git pull && git push` |
| `git_stash` | Stashes, runs command, then pops on local changes conflict | `git pull` → `git stash && git pull && git stash pop` |
| `git_commit_amend` | Amends last commit when nothing to commit | `git commit` → `git commit --amend --no-edit` |
| `git_rebase_no_changes` | Skips rebase step when no changes remain | `git rebase` → `git rebase --skip` |

---

## Configuration

Settings are stored in `~/.config/dangit/settings.json`. The file is created with defaults on first use, or you can create it manually.

### Default Settings

```json
{
  "rules": ["ALL"],
  "excludeRules": [],
  "aliasName": "dangit",
  "alterHistory": true,
  "waitCommand": 3,
  "debugMode": false
}
```

### Options Reference

| Setting | Type | Default | Description |
| ------- | ---- | ------- | ----------- |
| `rules` | `string[]` | `["ALL"]` | List of rule names to enable. Use `"ALL"` to enable all default rules. |
| `excludeRules` | `string[]` | `[]` | List of rule names to exclude, even if `"ALL"` is used. |
| `aliasName` | `string` | `"dangit"` | The name of the shell alias. |
| `alterHistory` | `bool` | `true` | Whether to add the corrected command to shell history. |
| `waitCommand` | `int` | `3` | Timeout in seconds to wait for command output. |
| `debugMode` | `bool` | `false` | Enable verbose debug output. |

### Examples

Enable only specific rules:

```json
{
  "rules": ["git_push", "git_add", "command_not_found", "sudo"]
}
```

Enable all rules but exclude some:

```json
{
  "rules": ["ALL"],
  "excludeRules": ["rm_dir", "sudo"]
}
```

---

## Custom Rules

dangit has a modular rule system. Each rule is an F# module with a `match` function and a `getNewCommand` function. To add your own rule, see the [Contributing Guide](CONTRIBUTING.md) for a step-by-step TDD workflow.

---

## Telemetry

dangit includes **optional, opt-in** telemetry powered by [OpenTelemetry](https://opentelemetry.io/). **No data is collected by default.**

### What's Collected (When Enabled)

- `dangit.corrections.applied` — count of corrections applied, with rule name
- `dangit.rules.evaluated` — number of rules evaluated per invocation
- `dangit.rules.matched` — number of rules that matched per invocation
- `dangit.correction.latency` — time in milliseconds to find corrections

No command content, file paths, or personal data is ever collected.

### How to Enable

Set the `DANGIT_TELEMETRY` environment variable:

```bash
export DANGIT_TELEMETRY=1
```

By default, telemetry is exported to the console (useful for local debugging). To export to Azure Monitor, also set:

```bash
export DANGIT_TELEMETRY_CONNECTION="InstrumentationKey=..."
```

---

## Project Structure

```
src/
├── Dangit.Cli/          # Entry point, argument parsing, Spectre.Console UI
├── Dangit.Core/         # Types, rule engine, configuration, shell abstractions
├── Dangit.Rules/        # All 20 correction rules
└── Dangit.Telemetry/    # OpenTelemetry instrumentation
tests/
└── Dangit.Tests/        # Unit & integration tests (Expecto)
```

---

## License

[MIT](LICENSE) © jtucker
