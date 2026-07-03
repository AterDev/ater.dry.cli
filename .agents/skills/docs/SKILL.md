---
name: docs
description: "Use for Perigon documentation work across this workspace, especially Perigon.docs/Content/docs/Perigon bilingual and versioned docs, syncing zh-CN and en-US content, updating docs from Perigon.CLI, Perigon.template, and Perigon.Modules changes, and deciding whether to revise existing pages or add new documentation."
---

# Perigon Documentation Workflow

## Workspace Scope

- The authoritative documentation lives under `Perigon.docs/Content/docs/Perigon`.
- Documentation is versioned by folder, then split by language: `zh-CN/<version>` and `en-US/<version>`.
- Source changes that drive Perigon docs usually come from `Perigon.CLI`, `Perigon.template`, and `Perigon.Modules`.

## Workflow

1. Identify the affected feature, command, template behavior, module behavior, or contributor workflow.
2. Check workspace changes in `Perigon.template`, `Perigon.CLI`, and `Perigon.Modules` first to determine what actually changed.
3. Ignore unrelated diffs and extract only Perigon-relevant behavior, constraints, commands, paths, version notes, or user-visible workflow changes.
4. Search existing pages under `Perigon.docs/Content/docs/Perigon` for the closest matching topic before creating new content.
5. Prefer updating or correcting an existing document when the topic already exists.
6. Create a new page only when the change introduces a clearly new concept, workflow, or reference topic.
7. Keep Chinese and English docs aligned in structure and meaning. When one side changes, update the matching page in the other language unless the user explicitly asks for one language only.
8. Place content in the correct version folder. If behavior differs by version, document the version-specific scope explicitly instead of silently overwriting another version's guidance.

## Conventions

- Preserve the existing section taxonomy and naming style under both `zh-CN` and `en-US`.
- Keep folder symmetry when equivalent pages exist in both languages.
- Document observable behavior from the repositories, not speculation or roadmap assumptions.
- Prefer concise task-oriented guidance, with commands, paths, prerequisites, and limitations called out directly.
- When adding a new page in a section that uses `.order`, update the corresponding `.order` file so navigation stays correct.
- If the source change is internal only and has no user, contributor, or operator impact, do not add noise to the docs.

## Search Targets

- `Perigon.docs/Content/docs/Perigon/zh-CN/<version>` for Chinese pages.
- `Perigon.docs/Content/docs/Perigon/en-US/<version>` for English pages.
- `Perigon.CLI/src`, `Perigon.template`, and `Perigon.Modules/src` for feature and workflow changes.
- README files, packaging scripts, command registration, AppHost/template structure, and module scripts when documenting usage or contributor flows.

## Validation

- Verify that new or updated pages are placed under the correct language and version directories.
- Verify that paired zh-CN and en-US pages remain semantically aligned.
- Verify commands, paths, and repository names against current workspace content before finishing.