module Dangit.Tests.GitRuleTests

open Expecto
open Dangit.Core
open Dangit.Rules

[<Tests>]
let gitRuleTests =
    testList "Git Rules" [
        testList "GitPush" [
            test "matches push with set-upstream hint" {
                let cmd = Command.withOutput "git push" "fatal: The current branch feature has no upstream branch.\nTo push the current branch and set the remote as upstream, use\n\n    git push --set-upstream origin feature\n"
                Expect.isTrue (Rule.isMatch cmd GitPush.rule) "Should match"
            }
            test "does not match successful push" {
                let cmd = Command.withOutput "git push" "Everything up-to-date"
                Expect.isFalse (Rule.isMatch cmd GitPush.rule) "Should not match"
            }
            test "extracts upstream command" {
                let cmd = Command.withOutput "git push" "fatal: ...\n    git push --set-upstream origin feature\n"
                let result = GitPush.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should have suggestion"
                Expect.exists result (fun s -> s.Contains("--set-upstream")) "Should contain set-upstream"
            }
        ]

        testList "GitAdd" [
            test "matches pathspec error" {
                let cmd = Command.withOutput "git commit -m 'fix'" "error: pathspec 'fix' did not match any file(s) known to git"
                Expect.isTrue (Rule.isMatch cmd GitAdd.rule) "Should match"
            }
            test "suggests git add for commit" {
                let cmd = Command.withOutput "git commit -m 'fix'" "did not match any file(s) known to git"
                let result = GitAdd.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should have suggestion"
                Expect.exists result (fun s -> s.Contains("git add")) "Should suggest git add"
            }
        ]

        testList "GitBranchDelete" [
            test "matches checked out branch" {
                let cmd = Command.withOutput "git branch -d main" "error: Cannot delete branch 'main' checked out"
                Expect.isTrue (Rule.isMatch cmd GitBranchDelete.rule) "Should match"
            }
            test "suggests checkout main first" {
                let cmd = Command.withOutput "git branch -D feature" "Cannot delete branch 'feature' checked out"
                let result = GitBranchDelete.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should have suggestion"
            }
        ]

        testList "GitBranchExists" [
            test "matches already exists" {
                let cmd = Command.withOutput "git checkout -b main" "fatal: a branch named 'main' already exists"
                Expect.isTrue (Rule.isMatch cmd GitBranchExists.rule) "Should match"
            }
            test "suggests checkout without -b" {
                let cmd = Command.withOutput "git checkout -b main" "already exists"
                let result = GitBranchExists.rule.GetNewCommand cmd
                Expect.equal result ["git checkout main"] "Should remove -b"
            }
        ]

        testList "GitPull" [
            test "matches rejected push" {
                let cmd = Command.withOutput "git push" "! [rejected] main -> main (non-fast-forward)\nerror: failed to push some refs\nhint: Updates were rejected because the tip of your current branch is behind\nhint: its remote counterpart. Integrate the remote changes (e.g.\nhint: 'git pull ...') before pushing again."
                Expect.isTrue (Rule.isMatch cmd GitPull.rule) "Should match"
            }
            test "suggests pull then push" {
                let cmd = Command.withOutput "git push" "rejected non-fast-forward"
                let result = GitPull.rule.GetNewCommand cmd
                Expect.equal result ["git pull && git push"] "Should suggest pull first"
            }
        ]

        testList "GitStash" [
            test "matches local changes conflict" {
                let cmd = Command.withOutput "git checkout main" "error: Your local changes to the following files would be overwritten by checkout"
                Expect.isTrue (Rule.isMatch cmd GitStash.rule) "Should match"
            }
            test "suggests stash + command + pop" {
                let cmd = Command.withOutput "git checkout main" "Your local changes would be overwritten"
                let result = GitStash.rule.GetNewCommand cmd
                Expect.equal result ["git stash && git checkout main && git stash pop"] "Should wrap with stash"
            }
        ]

        testList "GitCommitAmend" [
            test "matches nothing to commit" {
                let cmd = Command.withOutput "git commit -m 'test'" "nothing to commit, working tree clean"
                Expect.isTrue (Rule.isMatch cmd GitCommitAmend.rule) "Should match"
            }
            test "does not match amend" {
                let cmd = Command.withOutput "git commit --amend" "nothing to commit"
                Expect.isFalse (Rule.isMatch cmd GitCommitAmend.rule) "Should not match amend"
            }
            test "suggests amend" {
                let cmd = Command.withOutput "git commit -m 'test'" "nothing to commit"
                let result = GitCommitAmend.rule.GetNewCommand cmd
                Expect.equal result ["git commit --amend --no-edit"] "Should suggest amend"
            }
        ]

        testList "GitRebaseNoChanges" [
            test "matches no changes" {
                let cmd = Command.withOutput "git rebase main" "No changes - did you forget to use 'git add'?"
                Expect.isTrue (Rule.isMatch cmd GitRebaseNoChanges.rule) "Should match"
            }
            test "suggests rebase skip" {
                let cmd = Command.withOutput "git rebase main" "No changes"
                let result = GitRebaseNoChanges.rule.GetNewCommand cmd
                Expect.equal result ["git rebase --skip"] "Should suggest skip"
            }
        ]

        testList "GitDiffStaged" [
            test "matches empty git diff" {
                let cmd = Command.withOutput "git diff" ""
                Expect.isTrue (Rule.isMatch cmd GitDiffStaged.rule) "Should match empty diff"
            }
            test "suggests --staged" {
                let cmd = Command.withOutput "git diff" ""
                let result = GitDiffStaged.rule.GetNewCommand cmd
                Expect.equal result ["git diff --staged"] "Should suggest staged"
            }
        ]

        testList "GitUnknownCommand" [
            test "matches 'is not a git command' output" {
                let cmd = Command.withOutput "git puss" "git: 'puss' is not a git command. See 'git --help'.\n\nThe most similar command is\n\tpush"
                Expect.isTrue (Rule.isMatch cmd GitUnknownCommand.rule) "Should match unknown git command"
            }
            test "matches when output says not a git command without suggestion" {
                let cmd = Command.withOutput "git xyzzy" "git: 'xyzzy' is not a git command. See 'git --help'."
                Expect.isTrue (Rule.isMatch cmd GitUnknownCommand.rule) "Should match unknown git command without suggestions"
            }
            test "does not match valid git command" {
                let cmd = Command.withOutput "git push" "Everything up-to-date"
                Expect.isFalse (Rule.isMatch cmd GitUnknownCommand.rule) "Should not match valid git command"
            }
            test "does not match non-git command" {
                let cmd = Command.withOutput "docker puss" "docker: 'puss' is not a docker command."
                Expect.isFalse (Rule.isMatch cmd GitUnknownCommand.rule) "Should not match non-git command"
            }
            test "extracts suggestion from git output" {
                let cmd = Command.withOutput "git puss" "git: 'puss' is not a git command. See 'git --help'.\n\nThe most similar command is\n\tpush"
                let result = GitUnknownCommand.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should have suggestion"
                Expect.equal result.Head "git push" "Should suggest 'git push'"
            }
            test "extracts multiple suggestions from git output" {
                let cmd = Command.withOutput "git brnch" "git: 'brnch' is not a git command. See 'git --help'.\n\nThe most similar commands are\n\tbranch\n\tblame"
                let result = GitUnknownCommand.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should have suggestions"
                Expect.contains result "git branch" "Should contain 'git branch'"
            }
            test "preserves arguments after corrected subcommand" {
                let cmd = Command.withOutput "git puss origin main" "git: 'puss' is not a git command. See 'git --help'.\n\nThe most similar command is\n\tpush"
                let result = GitUnknownCommand.rule.GetNewCommand cmd
                Expect.equal result.Head "git push origin main" "Should preserve trailing args"
            }
            test "falls back to fuzzy match when no git suggestion" {
                let cmd = Command.withOutput "git psh" "git: 'psh' is not a git command. See 'git --help'."
                let result = GitUnknownCommand.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should fuzzy-match common subcommand"
                Expect.equal result.Head "git push" "Should suggest 'git push' via fuzzy match"
            }
            test "matches with output containing not found phrasing" {
                let cmd = Command.withOutput "git comit" "git: 'comit' is not a git command. See 'git --help'.\n\nThe most similar command is\n\tcommit"
                let result = GitUnknownCommand.rule.GetNewCommand cmd
                Expect.equal result.Head "git commit" "Should correct comit to commit"
            }
        ]
    ]
