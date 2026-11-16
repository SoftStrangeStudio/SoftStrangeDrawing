# Handoff

## Resume here

The next app change is the minimal, interaction-driven interface in `Assets/SoftStrange/Runtime/StudioView.cs`, with affected interaction/state owners in `DrawingApp.cs`, `ReferenceSheet.cs` and `HandInput.cs`. Inspect the existing source and Mac captures before editing. Preserve wrist-forward aiming, deliberate lock/edit, the independent grid, local imports and contextual advanced styles. The redesign is agreed direction, not implemented behavior. Render and click through the changed Mac workflow, then qualify it physically on Quest; a desktop capture does not establish headset acceptance.

## Identity and coordination — 2026-11-16

- Product: Soft Strange Drawing, an offline Quest MR reference app for physical paper and pencil/pen drawing.
- Workspace: `/Users/angelberger/Documents/SoftStrangeDrawing`.
- Repository: https://github.com/SoftStrangeStudio/SoftStrangeDrawing, branch `main`.
- Reliable Git identity: use `git rev-parse HEAD` and `git ls-remote origin refs/heads/main`; this handoff is versioned with its source. The pre-sync short revision was `31582aa`, tracking only README.md and LICENSE. App source/assets/tools/docs were local and untracked. Do not reset or overwrite this working tree to synchronize it.
- User confirmed that “main” means the GitHub main branch, and authorized source synchronization to this repository.
- Drive writes are explicitly on hold. Another agent is moving or copying the project into the Luminary shared drive's `Luminary-Projects`. Wait for that setup and verify the destination/records before updating tasks or handoff. Do not create a competing project or write to the older My Drive copy.
- Discovered older project: https://drive.google.com/drive/folders/1obt6-VGx5uz6vquEWYKJFieIoJRGshQH. Its ancestry is My Drive / Luminary Labs / Projects / Soft Strange Drawing; its `00 - Handoff`, `01 - Overview` and `02 - Planning` were empty when checked. This is not the verified Luminary shared-drive home.
- Verified shared-drive discovery anchor: https://drive.google.com/drive/folders/1IJJcE5889HEV4F4iN2kIAALxKAtqw9WT (`Luminary-Projects`, drive `0AFm3bXJpDncSUk9PVA`). No app home was found among its direct children at this check. Do not infer that the other agent has finished from this historical observation.

## What this chat did

1. Established the app's purpose: transparent/edge outlines in Quest passthrough for freehand physical drawing. Unity was implemented; the earlier all-Rust/OpenXR idea was considered but is not the app's current engine.
2. Built the Unity service/plugin structure, local image library, command API, diagnostic state recording and desktop input/house simulation. Development/desktop builds expose the existing loopback API and stdio MCP bridge; the mobile release disables the developer server and automatic pose recording.
3. Iterated hand input from finger/pinch-direction aiming to the accepted wrist-forward ray. Thumb/index midpoint remains the origin, with smoothing, bounded distance-based pinch thresholds, hysteresis/debounce and controller fallback. Physical comfort remains a separate acceptance step.
4. Added independent grid placement, reference lock/move/rotate/two-hand scale, pure white transparent recolorable assets, solid/dotted controls and bounded automatic Bezier conversion. Dense imports preserve the full solid raster rather than showing truncated curves.
5. Prepared 60 cleaned AI illustrations, 60 model subjects with 120 paired side/top views and original stylized models. AI originals/provenance remain in art; orthographic views are distinct from AI illustrations. Quiet original audio and decorative/reduced-motion behavior are present.
6. Authored a Blender house and desktop studio for local preview/capture. The existing replay directly transforms the reference; it is not a full tracked-hand simulation. House footage is desktop simulation, not genuine MR headset footage.
7. Built Quest candidates and investigated installation/startup with ADB. Earlier Quest Pro logs showed passthrough initialization and short active samples around 72 FPS, followed by idle. They did not prove the latest signed APK's stereo appearance, physical gestures or sustained performance.
8. Built a separate Steam Frame ARM64/OpenXR candidate and x86-64 Linux Flatpak. Native Frame acceptance remains open; emulated Linux launch aborted in Mono. These platform artifacts do not establish compatibility with actual hardware.
9. Cleaned Quest release configuration, pinned target SDK 34, incremented to 0.3.1/build 4 and created the explicitly authorized dedicated release key outside the repository. Release signing/package inspection passed. No store submission or publication was performed.
10. Rendered the current-source Mac app and clicked through its menus. Observed clutter from central translucent drawers over the artwork, weak selected states, four-subject paging and an upright reference disconnected from the paper. User requested almost no persistent UI, with direct interaction and subtle indicators. Replacement controls remain proposed.
11. Created and installed local development plugin `soft-strange-drawing-kit@personal` 0.1.1: one main router, 27 domain subskills and 22 tool plans. It guides normal conversational requests without formal change packets and maintains existing authoritative records after meaningful changes.

Coverage is the substantive work and decisions available in this chat plus the linked project records. Historical physical-device results are retained as historical reports; they were not repeated during this synchronization.

## Exact release artifact

- Local file: `builds/SoftStrangeDrawing-QuestRelease.apk`.
- Package/version: `com.softstrange.drawing`, 0.3.1, Android code 4.
- Size: 62,946,522 bytes.
- SHA-256: `8db3cbe81384450ddfa2b15234564f13e451d9dafb3626983d63a98f2ba7cb61`, rechecked 2026-11-16.
- ARM64 IL2CPP/Vulkan, minimum SDK 32 / target 34, non-development, dedicated v2/v3 release signing. Earlier package checks also covered 16 KB ZIP alignment.
- This binary is excluded from Git source synchronization with other builds. It remains local; no upload to Drive is claimed. The source can rebuild it using the existing private signing identity, but rebuilt bytes may differ.
- Signing secrets remain outside the repository at the protected local signing location. Never include them in a handoff, Git commit or automatic backup destination. A new signature cannot update an older test-signed installation in place; uninstalling deletes local data and is not automatically authorized.

## Working methods and ownership

- Read `AGENTS.md`, `memory.md` and the canonical plugin entry before app changes: `/Users/angelberger/plugins/soft-strange-drawing-kit/skills/soft-strange-drawing-kit/SKILL.md`.
- `python3 tools/headset.py build` builds the review APK. `python3 tools/headset.py build --release` builds and signs the release. Do not confuse the intermediate candidate with QuestRelease.
- `python3 tools/apk_preflight.py builds/SoftStrangeDrawing-QuestRelease.apk` inspects the actual package. Evaluate its JSON checks, not only the process exit status.
- `tools/headset.py validate` is specifically the review-build/developer-endpoint validation path, not a complete release acceptance test.
- BuildMac/BuildLinux/BuildSteamFrame methods in ProjectBuilder own target-specific builds; tools/build-flatpak.sh owns Flatpak packaging. Read docs/steam-frame-and-flatpak.md before those targets.
- The external canonical plugin source is preserved as a labeled recovery ZIP in packaging/codex, with hashes and recovery notes in docs/development-plugin.md. Do not maintain an extracted competing copy or edit installed caches.
- Actual current source/acceptance records: `memory.md`, `docs/validation.md`, `docs/store/readiness.md`, `docs/completion-checklist.md`. Inspect dates; historical sections are not proof of a later rebuild.

## Remaining tasks in dependency order

1. Wait for the other agent's shared-drive setup. Locate the actual project, task record and active Handoff, preserve their IDs, reconcile this state and verify write readback. No Drive project creation or duplicate tracker is authorized by this continuation.
2. Implement and visually validate the minimal UI: reference-centered affordances, clear placement and lock/edit, a quiet drawing view, readable contextual adjustments and faster browsing. Retain advanced styles and the grid.
3. Validate native Quest import/cancel/error handling and persistence; implement a safe, understandable per-image removal flow if adopted. Automated transfer uses APIs rather than file explorer automation.
4. Build the changed release, inspect it, then test both hands, controller switching, placement, two-hand handoff, reference/grid independence, OS recenter, interrupted grabs, sleep/wake and stereo rendering on the declared headset models.
5. Measure sustained CPU/GPU/memory/thermal behavior with dense imported curves and a real drawing session. Investigate the historical Meta capability errors and missing-script warnings on the actual current build.
6. Finish publisher/app ownership, public support/privacy, rights review, pricing, ratings, supported devices/play modes and genuine Quest media; review current official Meta requirements before an explicitly authorized submission. Signing/package checks alone are not store readiness.
7. Keep Frame and native Linux acceptance separate. Decide the first-release virtual-room boundary explicitly rather than silently treating a desktop simulator as a shipped Quest environment.

## Delivery boundaries

GitHub source synchronization is authorized. Drive writes are waiting on the other agent's destination setup. No app implementation, fresh Unity build, headset installation, store upload, sharing change or publication is part of this synchronization. Report exact commit/readback and Drive state separately in the final coordination update.
