---
name: ai-coding-customization
description: "Use when creating, reviewing, or updating AI coding instructions, Copilot custom instructions, AGENTS.md, .instructions.md, .agent.md, prompt files, or SKILL.md files. Applies OpenAI/Microsoft/GitHub-style customization patterns and Perigon.CLI placement conventions."
argument-hint: "customization task or target file"
---

# AI Coding Customization

## When To Use

Use this skill for repository AI coding enablement tasks, including:

- updating `.github/copilot-instructions.md`
- creating `.github/instructions/*.instructions.md`
- creating `.agents/skills/<name>/SKILL.md`
- creating or reviewing `.github/agents/*.agent.md`
- extracting architecture, style, workflow, or review guidance for future AI agents

## Distilled Public Patterns

Public OpenAI/Microsoft/GitHub ecosystem examples share these useful patterns:

- Put repository-wide agent guidance in a root or `.github` instruction file.
- Use `AGENTS.md` or `copilot-instructions.md` as the stable high-level contract for agents.
- Keep task-specific rules in separate `.instructions.md` files with narrow `applyTo` globs.
- Treat `description` as the discovery surface. Write it with explicit `Use when...` trigger words.
- Store repeatable workflows as skills with `SKILL.md`, optional `references`, `scripts`, and `assets` folders.
- Keep large background knowledge out of the skill entry point; progressively load it through local references.
- Do not copy external instructions verbatim. Re-express the pattern in project-specific language.

## Procedure

1. Classify the customization:
   - always-on repo behavior: `.github/copilot-instructions.md` or `AGENTS.md`
   - file/path-specific rule: `.github/instructions/*.instructions.md`
   - repeatable workflow: `.agents/skills/<skill-name>/SKILL.md`
   - focused persona/subagent: `.github/agents/*.agent.md`
2. Write frontmatter:
   - `SKILL.md`: `name` must match its folder and use lowercase hyphenated form.
   - `.instructions.md`: include a keyword-rich `description`; add `applyTo` only for automatic attachment.
   - `.agent.md`: include `description`, focused role instructions, and minimal tools when tools are specified.
3. Keep content concise and operational:
   - include when to use
   - include step-by-step procedure
   - include expected output or validation
   - link reference files with `./relative-path.md`
4. Validate:
   - check YAML frontmatter delimiters and quoting
   - check folder/file naming rules
   - check `applyTo` scope is not overly broad
   - avoid duplicating long repository docs when a concise extracted rule is enough

## Perigon.CLI Placement

- Put project-specific, always-available guidance in `.github/copilot-instructions.md`.
- Put C#/Razor/csproj architecture rules in `.github/instructions/perigon-architecture.instructions.md`.
- Put AI customization rules in `.github/instructions/ai-coding-customization.instructions.md`.
- Put general reusable skills in `.agents/skills`.

## Review Checklist

Before finishing an AI customization change, confirm:

- descriptions contain practical trigger words
- frontmatter is valid YAML
- skill folder name matches `name`
- instructions do not conflict with existing repository policy
- project facts were verified from local files
- external patterns were summarized, not copied
