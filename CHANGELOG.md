# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **CLI** — `dangit` entry point with Spectre.Console rich terminal UI
  - Automatic shell detection (PowerShell, Bash, Zsh)
  - Interactive selection prompt when multiple corrections match
  - `--command` / `--output` flags for explicit command correction
  - `--alias` / `alias` subcommand to print shell setup script
  - `rules` subcommand to list all rules and their status
  - `config` subcommand to display current configuration
  - `--version` and `--help` flags
- **Rule Engine** — evaluates all enabled rules, returns sorted unique corrections
- **Configuration** — JSON settings at `~/.config/dangit/settings.json`
  - Enable/disable rules individually or via `"ALL"`
  - Exclude specific rules
  - Custom alias name, history integration, wait timeout, debug mode
- **Core Rules (10)**
  - `cd_mkdir` — create missing directory then `cd` into it
  - `cd_parent` — fix missing space in `cd..`
  - `dry` — remove accidentally duplicated command word
  - `cat_dir` — replace `cat` with `ls` when target is a directory
  - `rm_dir` — add `-rf` flag when `rm` fails on a directory
  - `mkdir_p` — add `-p` flag for nested directory creation
  - `command_not_found` — suggest closest command via Levenshtein distance
  - `no_such_file` — suggest similar filenames in the directory
  - `sudo` — prepend `sudo` on permission denied errors
  - `fix_file` — add execute permission and retry
- **Git Rules (10)**
  - `git_push` — set upstream branch on first push
  - `git_add` — stage files when commit fails due to unstaged changes
  - `git_branch_delete` — check out main before deleting current branch
  - `git_branch_exists` — check out existing branch instead of creating
  - `git_checkout` — suggest similar branch names on pathspec error
  - `git_diff_staged` — add `--staged` when `git diff` returns empty
  - `git_pull` — pull before push on non-fast-forward rejection
  - `git_stash` — stash, run command, then pop on local changes conflict
  - `git_commit_amend` — amend last commit when nothing to commit
  - `git_rebase_no_changes` — skip rebase step when no changes remain
- **Shell Support** — PowerShell, Bash, and Zsh with alias generation
- **Telemetry** — optional, opt-in OpenTelemetry instrumentation
  - Metrics: corrections applied, rules evaluated/matched, correction latency
  - Tracing: activity spans for correction flow
  - Exporters: console (default) or Azure Monitor
  - Enabled via `DANGIT_TELEMETRY` environment variable
- **Build & Packaging**
  - Single-file, self-contained, trimmed executables
  - Cross-platform publish: win-x64, win-arm64, linux-x64, linux-arm64, osx-x64, osx-arm64
  - Central Package Management via `Directory.Packages.props`
  - SLNX solution format
- **Testing** — Expecto + FsCheck test suite covering rules, engine, CLI, configuration, shells, and telemetry

[Unreleased]: https://github.com/jtucker/dangit/commits/main
