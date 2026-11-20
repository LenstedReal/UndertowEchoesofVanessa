# Delivery audit — 2026-11-18

> **Historical record** of the previous delivery. For v0.3.0 (Unity 6000.3.27f1) see `README.md`, `CHANGES.md` and `KNOWN_ISSUES.md`.

## Included in this archive
- Unity 6000.0.23f1 project structure and HDRP 17.0.3 package manifest.
- 40 C# scripts across player, lantern, audio, puzzles, narrative, save, UI, world and editor tooling.
- Six Unity scene files (main menu, four acts, endings).
- Ten procedural OBJ meshes.
- Eighteen 4096×4096 PNG texture maps (six material sets × albedo/normal/mask).
- UHD output profile and Windows build/validation tooling.
- Existing EditMode test source.

## Checks performed in this packaging pass
- ZIP source extracted and inspected.
- `Packages/manifest.json` parsed as JSON.
- All 18 PNG texture files opened and dimensions checked as 4096×4096.
- Scene, C# script, and mesh file presence inventoried.
- Ending evaluation source inspected; it now refuses to resolve an ending until a final altar choice exists and counts unique normalized drawing IDs.
- Batch validator texture inventory updated to cover all shipped 18 maps.

## Not verified
- Unity Editor import/compilation.
- Unity EditMode test execution.
- Scene playability and physical collision behavior.
- Windows IL2CPP build output.
- 4K GPU performance, frame rate, VRAM use, or shader correctness on target hardware.

## Known production gaps
- Acts II–IV are scene/blockout foundations and still need complete environment art dressing and pacing.
- Hero character model, rig, facial animation, and final animation set are not included as finished production assets.
- Recorded voice performances and final licensed/original sound-bank content are not complete.
- The ocean mesh is a lightweight procedural visual surface, not a fully authored production ocean simulation.
- Final lighting, cinematic sequences, QA playthrough, accessibility, localization QA, and Steam release checks remain.

## 4K clarification
Texture maps are 4096×4096 and the player profile targets 3840×2160. This does not guarantee native 4K rendering at a stable frame rate. That must be profiled on the actual GPU with Unity HDRP.

## Fix pass 2026-11-19 (static review only; Unity not available in the review sandbox)
- Added HDRP assembly references to Undertow.Runtime / Undertow.Editor asmdefs (HDRP types were used with no reference).
- Corrected malformed 30-char shader GUID in 5 .mat files to HDRP/Lit.
- Fog override now created with typeof(Fog) (previous reflection used the wrong assembly and never ran).
- UHD profile: removed SetDirty(null); crouch now lowers the camera while standing still; HUD prompts localized (EN/TR) and Shift label corrected; altar lullaby choice can no longer be overwritten.
- Main menu now uses Resources/MainMenuKeyArt.png (concept key-art, 1672x941, letter-boxed) with hit areas over the baked Turkish entries.
- STILL NOT VERIFIED: Unity compile, EditMode tests, play-through, IL2CPP build, 4K frame rate. UndertowHDRPAsset_PC_Ultra.asset is hand-written YAML with an unverified script GUID: if the Game view is pink/black, run Window > Rendering > HDRP Wizard > Fix All and assign a freshly created HDRP asset.
