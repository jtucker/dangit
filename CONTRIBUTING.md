# Contributing to dangit

Thanks for your interest in contributing! This guide covers everything you need to get started.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (see `global.json` for the exact version)
- A terminal (PowerShell, Bash, or Zsh)
- Git

## Getting Started

```bash
git clone https://github.com/jtucker/dangit.git
cd dangit
dotnet build
dotnet test
```

All tests should pass before you start making changes.

---

## Project Structure

```
src/
├── Dangit.Cli/          # Entry point, argument parsing, Spectre.Console UI
├── Dangit.Core/         # Types, rule engine, configuration, shell abstractions
├── Dangit.Rules/        # All 20 correction rules (one file per rule)
└── Dangit.Telemetry/    # OpenTelemetry instrumentation
tests/
└── Dangit.Tests/        # Unit & integration tests (Expecto + FsCheck)
```

---

## Build & Test

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Publish a single-file executable for your platform
dotnet publish src/Dangit.Cli/Dangit.Cli.fsproj -c Release -r win-x64

# Publish for all platforms (PowerShell script)
./publish-all.ps1
```

---

## Adding a New Rule (TDD Workflow)

Each rule lives in its own `.fs` file under `src/Dangit.Rules/`. Follow this step-by-step process:

### 1. Write Failing Tests First

Create tests in `tests/Dangit.Tests/` (add to the appropriate test file or create a new one):

```fsharp
[<Tests>]
let myRuleTests =
    testList "MyRule" [
        test "matches when output contains expected error" {
            let cmd = Command.withOutput "some-command arg" "some error message"
            Expect.isTrue (MyRule.rule.Match cmd) "Should match"
        }

        test "does not match unrelated output" {
            let cmd = Command.withOutput "other-command" "success"
            Expect.isFalse (MyRule.rule.Match cmd) "Should not match"
        }

        test "produces correct suggestion" {
            let cmd = Command.withOutput "some-command arg" "some error message"
            let corrections = MyRule.rule.GetNewCommand cmd
            Expect.equal corrections ["corrected-command arg"] "Should suggest fix"
        }
    ]
```

Run `dotnet test` — tests should **fail** (red).

### 2. Implement the Rule

Create `src/Dangit.Rules/MyRule.fs`:

```fsharp
namespace Dangit.Rules

open Dangit.Core

module MyRule =
    let private matchCmd (cmd: Command) =
        // Match logic: inspect cmd.Script, cmd.Output, cmd.ScriptParts
        match cmd.Output with
        | Some output -> output.Contains("some error message")
        | None -> false

    let private getNewCommand (cmd: Command) =
        // Return list of corrected command strings
        [ "corrected-command" ]

    let rule =
        Rule.create "my_rule" matchCmd getNewCommand
```

**Important:** Add the `<Compile Include="MyRule.fs" />` entry to `src/Dangit.Rules/Dangit.Rules.fsproj`. Order matters in F# — place it after any modules it depends on.

### 3. Register the Rule

Add your rule to the `allRules` list in `src/Dangit.Cli/CliLogic.fs`:

```fsharp
let allRules : Rule list =
    [ // ... existing rules ...
      Dangit.Rules.MyRule.rule ]
```

### 4. Run Tests Again

```bash
dotnet test
```

Tests should now **pass** (green). ✅

### 5. Optional: Set Rule Properties

```fsharp
let rule =
    Rule.create "my_rule" matchCmd getNewCommand
    |> Rule.withPriority 900              // Lower = higher priority (default: 1000)
    |> Rule.withRequiresOutput false      // Match even without command output
    |> Rule.withEnabledByDefault false    // Disabled unless explicitly enabled
```

---

## Code Style

- **Language:** F# 10 targeting .NET 10
- **Formatting:** Follow the `.editorconfig` in the repo root
- **Conventions:**
  - Use `camelCase` for local bindings and parameters
  - Use `PascalCase` for module-level functions and types
  - Prefer pure functions; side effects at the edges only
  - Use pattern matching over if/else chains where practical
  - Keep rule modules self-contained (one file per rule)
- **Testing:** [Expecto](https://github.com/haf/expecto) with [FsCheck](https://fscheck.github.io/FsCheck/) for property-based tests
- **UI:** [Spectre.Console](https://spectreconsole.net/) for terminal rendering

---

## Pull Request Process

1. **Fork & branch** — create a feature branch from `main` (e.g., `feature/my-new-rule`)
2. **Make changes** — keep commits focused and atomic
3. **Test** — ensure `dotnet test` passes with no regressions
4. **Build** — ensure `dotnet build` produces no warnings (`TreatWarningsAsErrors` is enabled)
5. **Open a PR** — describe what your change does and why
6. **Review** — address any feedback from maintainers

### Commit Message Convention

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add new_rule for handling XYZ errors
fix: correct matching logic in git_push rule
docs: update README with new rule reference
test: add property tests for command_not_found
refactor: simplify rule engine pipeline
```

---

## Reporting Issues

Found a bug or have a suggestion? [Open an issue](https://github.com/jtucker/dangit/issues/new) with:

- What you expected to happen
- What actually happened
- Shell and OS info (`dangit --version`, `$PSVersionTable` or `bash --version`)
- Steps to reproduce

---

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
