# Release Validation Scripts

`verify-release-package.sh` and `verify-release-package.cmd` are equivalent host
wrappers for the same repository release-validation contract.

- Use `verify-release-package.sh` on Bash-capable hosts and in the Ubuntu GitHub
  Actions package-validation job.
- Use `verify-release-package.cmd` from Windows Command Prompt; Bash and Python
  are not required.

Both wrappers delegate substantive checks to checked-in C# maintenance tools,
run the package-reference-only fresh consumer from an isolated temporary
directory, and execute the sample's non-interactive validation path.

The wrappers accept an optional artifact directory. The default is `artifacts`.

```text
.github\scripts\verify-release-package.cmd artifacts
bash .github/scripts/verify-release-package.sh artifacts
```
