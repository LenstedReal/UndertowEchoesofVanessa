# UNDERTOW: ECHOES OF VANESSA — Unity HDRP project

> **Historical record.** This file describes the 18 November 2026 delivery (Unity 6000.0.23f1, HDRP 17.0.3). The current project is v0.3.0 on Unity 6000.3.27f1; see `README.md`, `CHANGES.md` and `KNOWN_ISSUES.md`.

## Engine
- Unity Editor: 6000.0.23f1
- Render pipeline: HDRP 17.0.3
- Target: Windows x86_64
- UHD profile: 3840x2160 output target; actual 4K performance depends on GPU and scene settings.

## Open
1. Extract this archive.
2. In Unity Hub choose **Add > Add project from disk** and select this folder.
3. Install/use Unity Editor **6000.0.23f1** with Windows Build Support (IL2CPP).
4. Allow Package Manager to resolve `Packages/manifest.json`.
5. Open `Assets/_Project/Scenes/MainMenu.unity`.
6. Use **Undertow > Build All Canonical Scenes & Prefabs** to regenerate the six deterministic greybox scenes.
7. Run **Undertow > Validate All** (if available in this project menu) and run EditMode tests.
8. Apply **Undertow > Apply UHD 4K PC Profile** before creating a Windows build.

## Important status
This is an advanced playable-prototype foundation, not a finished commercial 3–4 hour game.
The four act scenes are generated greybox/blockout environments. The included procedural meshes and textures are original project assets, but they do not replace bespoke, production-quality hero models, character rigs, motion-captured animation, recorded voice acting, final level art dressing, or a full sound mix.
Unity Editor compilation, EditMode test execution, Windows executable creation, GPU profiling, and playtesting were not run in this packaging environment. Do not interpret a configured build target as a verified build.
