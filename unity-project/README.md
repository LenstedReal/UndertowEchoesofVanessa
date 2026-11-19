# Undertow: Echoes of Vanessa — Unity 6 HDRP

**Unity Editor:** `6000.0.23f1`  
**Render Pipeline:** HDRP `17.0.3`  
**Target:** Windows x86-64  
**Visual target:** 3840×2160 UHD output, 4K source PBR maps, HDRP temporal anti-aliasing, volumetric atmosphere, high-quality shadows and texture mip streaming.

> This is an ambitious playable prototype / vertical-slice project, not a finished 3–4 hour photorealistic commercial game. Unity Editor compilation, runtime, target-GPU performance and Windows binary creation still have to be verified on a machine with Unity installed.

## Open the project

1. Extract the complete project folder.
2. Install Unity Hub and Unity Editor `6000.0.23f1` with Windows Build Support (IL2CPP).
3. In Unity Hub choose **Add > Add project from disk** and select the folder containing `Assets`, `Packages` and `ProjectSettings`.
4. Wait for package resolution and asset import. The first import of 18 4K texture maps may take several minutes and requires substantial disk/RAM/GPU memory.
5. Open the project and wait for compilation to finish.
6. Run **Undertow > Configure 4K Texture & Mesh Importers** once if Unity has not applied the import settings automatically.
7. Run **Undertow > Build All Canonical Scenes & Prefabs**. This regenerates the six scene hierarchies and generated HDRP materials from the included Editor tool.
8. Open `Assets/_Project/Scenes/ActI_MercifulRequiem.unity` and press Play.

## 4K asset configuration

`Assets/_Project/Textures/` contains 18 4096×4096 PNG maps across six PBR sets:

- Wet aged deck wood (Albedo, Normal, Mask)
- Antique brass (Albedo, Normal, Mask)
- Torn black sail (Albedo, Normal, Mask)
- Sunken temple stone (Albedo, Normal, Mask)
- Storm ocean (Albedo, Normal, Mask)
- Underwater water (Albedo, Normal, Mask)

The importer configures HQ compression, mipmaps, streaming mipmaps, anisotropic filtering, normal-map import type and linear mask-map color space. 4K textures do not by themselves guarantee photorealism or 4K frame-rate. The target GPU must be profiled; use High/Medium settings or lower output resolution on GPUs that cannot sustain UHD.

## Scenes and current gameplay systems

- `MainMenu`: three save slots, new game and continue flow.
- `ActI_MercifulRequiem`: galleon deck, hull, railings, sails, cabins, lantern, fuel, surgeon hook, five-bell sequence, cargo hatch and crate-ring interactions.
- `ActII_ShipGraveyard`: wrecked hulls, narrow deck route, ocean surface, lantern-count bell sequence, Echo/Mimic route decision and crate ring.
- `ActIII_ForsakenPort`: flooded street, building fronts, clock tower, Crane ledger and mirror-room decision.
- `ActIV_SunkenTemple`: basalt nave, columns, mirrors, prism/lens pickups, underwater particles and leviathan movement.
- `Endings_Resolution`: behavior-dependent Answer, Surface and Undertow ending tableaux.

The included models are original procedural OBJ assets. They are suitable for this prototype but do not replace bespoke high-poly hero assets, detailed UV/material authoring, animation rigs, facial animation or professional environment art. The player is first-person; movement, camera sway, lantern motion, sails, floating debris, storm flashes and the leviathan have procedural motion. There are no animated humanoid NPCs in this build.

## Controls

- WASD: move
- Mouse: look
- E: interact
- F: toggle lantern
- Q or right mouse: listen
- C or Left Ctrl: crouch
- Shift: cautious movement
- Esc: pause

## Validation and Windows build

- **Undertow > Validate Project References & Build Readiness** checks scene hierarchies, imported OBJ meshes, 4K texture import settings and authored material maps.
- **Window > General > Test Runner > EditMode** runs the NUnit suite for progression, puzzles, endings, save recovery, lantern and sanity systems.
- **Undertow > Build Windows 64-bit Development Player** creates a diagnostic Windows build.
- **Undertow > Build Windows 64-bit Release Player** creates the compressed release candidate under `Builds/Windows64/Release/`.

The automated DevOps configuration runs scene generation, EditMode tests, asset/hierarchy validation and the Windows Release build. No successful Unity compile, runtime walkthrough, visual QA, FPS result or EXE is claimed until those steps actually execute on a Unity 6 Windows-capable runner.

## Important technical notes

- Project uses the legacy Unity Input Manager for the current controller and keeps the Input System package available for future migration.
- Campaign state is a persistent singleton independent of scene-local systems.
- Puzzle completion, route decisions, drawings, player transform, lantern fuel and sanity are persisted; puzzle controllers restore solved states from campaign data on scene reload.
- Act II requires a committed Echo or Mimic route before progression. Act III requires the canonical clock and a decision after the Crane ledger is found. Act IV requires mirror alignment, prism, lens and a lit lantern at the altar.
- Save data uses three slots, SHA-256 payload verification, temporary-file writes and backup recovery.
- HUD and menus use Unity IMGUI for a functional prototype and still need a final UI art pass.
- Audio has procedural fallback systems. Recorded dialogue, licensed ambience/music and final FMOD banks are not included.
