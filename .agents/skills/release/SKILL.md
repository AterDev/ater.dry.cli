---
name: release
description: Prepare and publish Perigon.CLI patch releases, including versioning, release notes, bilingual docs, validation, and the nuget-branch CI handoff.
---

# Perigon.CLI Release Workflow

Use this skill when releasing `Perigon.CLI` or when the user asks to package and publish a version. The NuGet publishing workflow runs automatically when `origin/nuget` receives a push.

## Before editing

- Inspect `git status` in both `C:\codes\Perigon.CLI` and `C:\codes\Perigon.docs`.
- Preserve unrelated user changes. Stage only files belonging to the release.
- Confirm the exact current version from `src/Apps/CommandLine/CommandLine.csproj`.

## Release contents

- Unless the user specifies a version, increment only the patch component of `<Version>` in `src/Apps/CommandLine/CommandLine.csproj`.
- Keep `src/Apps/Dashboard/Dashboard.csproj` on the same version.
- Update `<PackageReleaseNotes>` with only the changes being released.
- Update the matching Chinese and English pages under `Perigon.docs/Content/docs/Perigon`, normally the command-line page and the version update notes. Keep their structure and meaning aligned.
- Add or update reusable release guidance in this skill and `Contribution.md` when the workflow changes.

## Validation

Run the focused tests for the changed feature, then the full test project:

```powershell
dotnet test tests/StudioMod.Tests/CoreMod.Tests.csproj --configuration Release --verbosity minimal
```

The CI-equivalent package/studio build is:

```powershell
pwsh ./scripts/PublishToLocal.ps1 -withStudio:$true
```

`PublishToLocal.ps1` reads the tracked `src/Apps/CommandLine/ShareDlls.txt` when packaging Studio and removes the listed duplicate DLLs. If project dependencies change, run `scripts/CheckSharedDlls.ps1` to refresh the list and commit the updated file.

Run it only after tests pass; it creates local publish/package artifacts and may install the package locally.

## Branch handoff

1. Commit the Perigon.CLI release changes on the current development branch with a conventional emoji-prefixed message such as `🚀 release: prepare Perigon.CLI 10.1.12`.
2. Push that branch if it is the requested source branch.
3. Fetch `origin/nuget`, verify it has no unexpected divergence, merge the release commit into a local `nuget` branch, and push `nuget` without force-pushing.
4. Confirm `.github/workflows/publish-nuget.yml` is triggered by the push and report the Actions run URL/status.

If the `nuget` branch has unrelated commits or conflicts, stop before merging and report the exact divergence. Do not reset, discard, or force-push user work.

## Docs repository

Commit and push only the release-related files in `Perigon.docs`; leave any pre-existing uncommitted documentation changes untouched. Verify both `zh-CN` and `en-US` pages before handing off.
