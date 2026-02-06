module Dangit.Tests.CoreRuleTests

open Expecto
open Dangit.Core
open Dangit.Rules

[<Tests>]
let coreRuleTests =
    testList "Core Rules" [
        testList "CdMkdir" [
            test "matches cd with 'no such file' output" {
                let cmd = Command.withOutput "cd mydir" "bash: cd: mydir: No such file or directory"
                Expect.isTrue (Rule.isMatch cmd CdMkdir.rule) "Should match"
            }
            test "does not match successful cd" {
                let cmd = Command.withOutput "cd mydir" ""
                Expect.isFalse (Rule.isMatch cmd CdMkdir.rule) "Should not match"
            }
            test "does not match non-cd command" {
                let cmd = Command.withOutput "ls mydir" "No such file or directory"
                Expect.isFalse (Rule.isMatch cmd CdMkdir.rule) "Should not match"
            }
            test "suggests mkdir -p && cd" {
                let cmd = Command.withOutput "cd mydir" "No such file or directory"
                let result = CdMkdir.rule.GetNewCommand cmd
                Expect.equal result ["mkdir -p mydir && cd mydir"] "Should suggest mkdir + cd"
            }
            test "matches 'does not exist' output" {
                let cmd = Command.withOutput "cd foo" "cd: foo: does not exist"
                Expect.isTrue (Rule.isMatch cmd CdMkdir.rule) "Should match does not exist"
            }
            test "matches Windows error" {
                let cmd = Command.withOutput "cd foo" "The system cannot find the path specified"
                Expect.isTrue (Rule.isMatch cmd CdMkdir.rule) "Should match Windows error"
            }
            test "does not match cd without output" {
                let cmd = Command.withoutOutput "cd mydir"
                Expect.isFalse (Rule.isMatch cmd CdMkdir.rule) "Should not match without output"
            }
        ]

        testList "CdParent" [
            test "matches cd.." {
                let cmd = Command.withoutOutput "cd.."
                Expect.isTrue (Rule.isMatch cmd CdParent.rule) "Should match"
            }
            test "does not match cd .." {
                let cmd = Command.withoutOutput "cd .."
                Expect.isFalse (Rule.isMatch cmd CdParent.rule) "Should not match"
            }
            test "suggests cd .." {
                let cmd = Command.withoutOutput "cd.."
                let result = CdParent.rule.GetNewCommand cmd
                Expect.equal result ["cd .."] "Should add space"
            }
            test "matches cd../foo" {
                let cmd = Command.withoutOutput "cd../foo"
                Expect.isTrue (Rule.isMatch cmd CdParent.rule) "Should match cd../foo"
            }
            test "suggests cd ../foo" {
                let cmd = Command.withoutOutput "cd../foo"
                let result = CdParent.rule.GetNewCommand cmd
                Expect.equal result ["cd ../foo"] "Should add space in path"
            }
            test "does not require output" {
                Expect.isFalse CdParent.rule.RequiresOutput "Should not require output"
            }
        ]

        testList "Dry" [
            test "matches git git status" {
                let cmd = Command.withoutOutput "git git status"
                Expect.isTrue (Rule.isMatch cmd Dry.rule) "Should match double command"
            }
            test "does not match git status" {
                let cmd = Command.withoutOutput "git status"
                Expect.isFalse (Rule.isMatch cmd Dry.rule) "Should not match normal command"
            }
            test "removes duplicate" {
                let cmd = Command.withoutOutput "git git status"
                let result = Dry.rule.GetNewCommand cmd
                Expect.equal result ["git status"] "Should remove duplicate"
            }
            test "matches npm npm install" {
                let cmd = Command.withoutOutput "npm npm install"
                Expect.isTrue (Rule.isMatch cmd Dry.rule) "Should match npm npm"
            }
            test "removes npm duplicate" {
                let cmd = Command.withoutOutput "npm npm install"
                let result = Dry.rule.GetNewCommand cmd
                Expect.equal result ["npm install"] "Should remove npm duplicate"
            }
            test "does not require output" {
                Expect.isFalse Dry.rule.RequiresOutput "Should not require output"
            }
            test "has priority 900" {
                Expect.equal Dry.rule.Priority 900 "Should have priority 900"
            }
        ]

        testList "CatDir" [
            test "matches cat on directory" {
                let cmd = Command.withOutput "cat mydir/" "cat: mydir/: Is a directory"
                Expect.isTrue (Rule.isMatch cmd CatDir.rule) "Should match"
            }
            test "suggests ls" {
                let cmd = Command.withOutput "cat mydir/" "Is a directory"
                let result = CatDir.rule.GetNewCommand cmd
                Expect.equal result ["ls mydir/"] "Should suggest ls"
            }
            test "does not match cat on file" {
                let cmd = Command.withOutput "cat file.txt" "contents of file"
                Expect.isFalse (Rule.isMatch cmd CatDir.rule) "Should not match file"
            }
            test "does not match non-cat command" {
                let cmd = Command.withOutput "ls mydir/" "Is a directory"
                Expect.isFalse (Rule.isMatch cmd CatDir.rule) "Should not match ls"
            }
        ]

        testList "RmDir" [
            test "matches rm on directory" {
                let cmd = Command.withOutput "rm mydir" "rm: mydir: is a directory"
                Expect.isTrue (Rule.isMatch cmd RmDir.rule) "Should match"
            }
            test "does not match rm -rf" {
                let cmd = Command.withOutput "rm -rf mydir" "is a directory"
                Expect.isFalse (Rule.isMatch cmd RmDir.rule) "Should not match -rf"
            }
            test "does not match rm -r" {
                let cmd = Command.withOutput "rm -r mydir" "is a directory"
                Expect.isFalse (Rule.isMatch cmd RmDir.rule) "Should not match -r"
            }
            test "suggests rm -rf" {
                let cmd = Command.withOutput "rm mydir" "is a directory"
                let result = RmDir.rule.GetNewCommand cmd
                Expect.equal result ["rm -rf mydir"] "Should add -rf"
            }
            test "does not match non-rm command" {
                let cmd = Command.withOutput "ls mydir" "is a directory"
                Expect.isFalse (Rule.isMatch cmd RmDir.rule) "Should not match ls"
            }
        ]

        testList "MkdirP" [
            test "matches mkdir without -p" {
                let cmd = Command.withOutput "mkdir a/b/c" "mkdir: cannot create directory 'a/b/c': No such file or directory"
                Expect.isTrue (Rule.isMatch cmd MkdirP.rule) "Should match"
            }
            test "does not match mkdir -p" {
                let cmd = Command.withOutput "mkdir -p a/b/c" "No such file or directory"
                Expect.isFalse (Rule.isMatch cmd MkdirP.rule) "Should not match -p"
            }
            test "suggests mkdir -p" {
                let cmd = Command.withOutput "mkdir a/b/c" "cannot create directory"
                let result = MkdirP.rule.GetNewCommand cmd
                Expect.equal result ["mkdir -p a/b/c"] "Should add -p"
            }
            test "matches 'No such file or directory' output" {
                let cmd = Command.withOutput "mkdir deep/nested" "No such file or directory"
                Expect.isTrue (Rule.isMatch cmd MkdirP.rule) "Should match no such file"
            }
        ]

        testList "CommandNotFound" [
            test "matches command not found" {
                let cmd = Command.withOutput "gti status" "gti: command not found"
                Expect.isTrue (Rule.isMatch cmd CommandNotFound.rule) "Should match"
            }
            test "matches Windows not recognized" {
                let cmd = Command.withOutput "gti status" "'gti' is not recognized as an internal or external command"
                Expect.isTrue (Rule.isMatch cmd CommandNotFound.rule) "Should match Windows error"
            }
            test "suggests similar commands" {
                let cmd = Command.withOutput "gti status" "gti: command not found"
                let result = CommandNotFound.rule.GetNewCommand cmd
                Expect.isNonEmpty result "Should suggest alternatives"
                Expect.exists result (fun s -> s.StartsWith("git")) "Should suggest git"
            }
            test "preserves arguments in suggestion" {
                let cmd = Command.withOutput "gti push origin main" "gti: command not found"
                let result = CommandNotFound.rule.GetNewCommand cmd
                Expect.exists result (fun s -> s.Contains("push origin main")) "Should preserve args"
            }
            test "does not match normal output" {
                let cmd = Command.withOutput "git status" "On branch main"
                Expect.isFalse (Rule.isMatch cmd CommandNotFound.rule) "Should not match normal output"
            }
            test "has priority 800" {
                Expect.equal CommandNotFound.rule.Priority 800 "Should have priority 800"
            }
            test "matches unknown command" {
                let cmd = Command.withOutput "foo" "unknown command: foo"
                Expect.isTrue (Rule.isMatch cmd CommandNotFound.rule) "Should match unknown command"
            }
        ]

        testList "SudoCommand" [
            test "matches permission denied" {
                let cmd = Command.withOutput "apt install vim" "E: Could not open lock file - permission denied"
                Expect.isTrue (Rule.isMatch cmd SudoCommand.rule) "Should match"
            }
            test "does not match already sudo" {
                let cmd = Command.withOutput "sudo apt install vim" "permission denied"
                Expect.isFalse (Rule.isMatch cmd SudoCommand.rule) "Should not match sudo"
            }
            test "prepends sudo" {
                let cmd = Command.withOutput "apt install vim" "permission denied"
                let result = SudoCommand.rule.GetNewCommand cmd
                Expect.equal result ["sudo apt install vim"] "Should prepend sudo"
            }
            test "matches access denied" {
                let cmd = Command.withOutput "rm /etc/hosts" "access denied"
                Expect.isTrue (Rule.isMatch cmd SudoCommand.rule) "Should match access denied"
            }
            test "matches operation not permitted" {
                let cmd = Command.withOutput "kill -9 1" "operation not permitted"
                Expect.isTrue (Rule.isMatch cmd SudoCommand.rule) "Should match operation not permitted"
            }
            test "matches Windows access is denied" {
                let cmd = Command.withOutput "del system32" "Access is denied"
                Expect.isTrue (Rule.isMatch cmd SudoCommand.rule) "Should match Windows error"
            }
            test "has priority 900" {
                Expect.equal SudoCommand.rule.Priority 900 "Should have priority 900"
            }
        ]

        testList "FixFile" [
            test "matches permission denied on script" {
                let cmd = Command.withOutput "./script.sh" "bash: ./script.sh: Permission denied"
                Expect.isTrue (Rule.isMatch cmd FixFile.rule) "Should match"
            }
            test "suggests chmod +x" {
                let cmd = Command.withOutput "./script.sh" "Permission denied"
                let result = FixFile.rule.GetNewCommand cmd
                Expect.equal result ["chmod +x ./script.sh && ./script.sh"] "Should chmod and run"
            }
            test "does not match multi-word commands" {
                let cmd = Command.withOutput "python script.py" "Permission denied"
                Expect.isFalse (Rule.isMatch cmd FixFile.rule) "Should not match multi-word"
            }
            test "does not match sudo commands" {
                let cmd = Command.withOutput "sudo ./script.sh" "Permission denied"
                Expect.isFalse (Rule.isMatch cmd FixFile.rule) "Should not match sudo"
            }
        ]
    ]
