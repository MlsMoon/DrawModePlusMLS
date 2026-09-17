# Changelog

All notable changes to this project are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project uses [Semantic Versioning](https://semver.org/).

## [0.2.0] - 2026-09-17

### Added

- Agent entry points: `AGENTS.md`, `llms.txt`, and `Skills~/README.md`.
- In-package skills: `drawmodeplus-use-plugin` and `drawmodeplus-develop-plugin`.
- GitHub community files: `LICENSE`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`,
  `SECURITY.md`, `THIRD_PARTY.md`, and issue / pull-request templates.
- UPM manifest `package.json` (`com.mlsmoon.drawmodeplus`) for git URL installs.
- `Docs/` translations and screenshot GIFs. `Example/` demo scene and prefabs.

### Changed

- Root `README.md` is English only. Chinese moved to `Docs/README.zh-Hans.md`.
- Renamed `Images/` to `Docs/images/` and moved `Demo.unity` / `Prefabs/` into `Example/`.
- Control panel menu is now `Tools/DrawModePlus/Draw Mode Control Panel`.
- `ResourceFinder` resolves the plugin root from `DrawModePlus.asmdef` and loads
  textures from `Arts/Textures` (Assets copy and UPM).
- Comments, XML docs, and shader notes are English.

### Fixed

- Texture lookup no longer assumes a top-level `Textures/` folder or a Windows
  compile-time `Assets\` path.

## [0.1.0] - 2026-08-24

### Added

- SceneView draw modes: Depth, WorldNormal (Forward / Deferred), BaseColor,
  MaterialAO, Metallic, Roughness, TexelDensity, UV0, Stencil, Reflection.
- `DrawModePlusRendererFeature` with editor-time auto-inject into URP Renderer Data.
- Control panel and SceneView overlay for Depth range and Stencil compare.
- Reflection override redraw (`DrawModePlus/ReflectionView`) with Force Mirror
  and a global roughness slider.
- TexelDensity host LightMode `DrawModePlusTexelDensity` plus FlatGray fallback.
- Demo scene, sample meshes, and documentation GIFs.
