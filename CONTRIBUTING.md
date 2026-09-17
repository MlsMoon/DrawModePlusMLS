# Contributing

Thanks for helping improve DrawModePlusMLS.

## Before you start

1. Read [`AGENTS.md`](AGENTS.md).
2. Load the matching skill:
   - Integration / docs / host shaders → `Skills~/drawmodeplus-use-plugin`
   - Runtime or Editor source → `Skills~/drawmodeplus-develop-plugin`
3. Open an issue first for large features or breaking contract changes.

## Development setup

1. Use Unity 2022.3+ with URP 14.
2. Copy this repository under `Assets/` or add it from the git URL.
3. Wait for compile. Confirm `DrawModePlusRendererFeature` is injected.
4. Open `Example/Demo.unity` and cycle every SceneView mode.

Do not add Unity Test Runner assemblies or play-mode test scripts unless a
maintainer asks for them.

## Coding rules

- Public API, comments, commit messages, skills, and the root README stay English.
- Translations go in `Docs/`.
- Match existing naming: `m_` is not used here; keep current field style.
- Verify method signatures in this repo before calling Unity / URP APIs.
- Keep Player builds free of required debug passes. Gate editor-only work with
  `UNITY_EDITOR` as the existing Renderer Feature already does.

## Pull requests

- One concern per PR.
- Use [Conventional Commits](https://www.conventionalcommits.org/) in English:
  `type(scope): description`
- Update `CHANGELOG.md` under `[Unreleased]` or the next version.
- If you add a draw mode, follow `Skills~/drawmodeplus-develop-plugin/references/adding-a-mode.md`.
- Fill in the pull-request template.

## Reporting bugs

Use the bug report template. Include Unity version, URP version, rendering path
(Forward / Deferred), the selected draw mode, and whether SceneView or Game View
is affected.
