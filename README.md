# Soft Strange Drawing

A cozy mixed reality reference studio for drawing on physical paper. Built with Unity 6000.3.11f1, OpenXR, XR Hands and Unity OpenXR Meta. The current signed candidate is **0.3.1 (Android build 4)**.

Start with the [current handoff](docs/handoff.md) and [completion checklist](docs/completion-checklist.md). The requested minimal, interaction-driven UI redesign remains pending; the menus described below are the existing implementation.

## Draw

Choose a cleaned AI illustration or a side/top model reference. The offline library contains 60 AI illustrations and 60 model subjects with 120 paired views. Start drawing, pinch near the outline to place it, use both hands to resize, then Lock. Trace with your own pen or pencil. The drawing dock has three controls: Outlines, Lock/Unlock and Palette.

Palette opens one drawer at a time: Ink controls solid/dotted strokes, line width, opacity and color; Grid provides an independently movable guide; Studio contains sound, reduced motion, recenter, help and diagnostics. White outlines have transparent interiors and backgrounds. Imported photos use edge extraction. Small animated 3D models preview the selected subject on Home. Original soft sound cues and a quiet loop can be muted.

The pointer begins between thumb and index tips and aims along the wrist’s forward axis, independent of finger curl. It smooths movement and fades in as the pinch closes. Pinch thresholds scale within a bounded hand-size range, with separate press/release distances. Tracking loss, reserved system gestures and system focus loss cancel interaction. Controllers use trigger for menus and grip near the outline for placement.

## Build and run

- Quest review APK: `python3 tools/headset.py build` → `builds/SoftStrangeDrawing.apk`.
- Connect an awake Quest with Developer Mode and USB debugging allowed, then `python3 tools/headset.py install`. This preserves saved images with `adb install -r`.
- Validate while wearing the headset: `python3 tools/headset.py validate --duration 30`.
- USB upload: `python3 tools/headset.py upload /absolute/path/reference.png`. The helper uses `http://localhost:8790` forwarded to the headset's port 8787, leaving Mac's port 8787 free.
- Mac: Unity menu **Soft Strange > Build Mac Studio**, then launch `builds/Mac/Soft Strange Drawing.app`.
- Non-development APK: Unity menu **Soft Strange > Build Quest release candidate** → `builds/SoftStrangeDrawing-ReleaseCandidate.apk`.
- Signed Quest release: `python3 tools/headset.py build --release` → `builds/SoftStrangeDrawing-QuestRelease.apk`, using the existing private release identity.

The non-development build has no developer upload server, no automatic pose recording and no Internet/raw-camera/microphone permission. Its bundled library works offline; native image import remains available. The review and intermediate candidate use test signing; `SoftStrangeDrawing-QuestRelease.apk` uses the dedicated release identity. Signing is complete for that artifact, but hardware acceptance, publisher details and Meta submission remain open. Review [store readiness](docs/store/readiness.md) before distribution. Signing credentials are kept outside this repository and are not included in source synchronization.

## Desktop controls and developer API

Mouse click selects controls; drag near an unlocked outline moves it; wheel resizes. Right drag looks around, WASD moves, R brings the outline/menu here, Space locks/unlocks, Escape opens Home, F1 toggles diagnostics. The authored house provides desktop context, not a live passthrough simulation or proof of Quest behavior.

Review builds expose loopback-only `GET /api/state`, `GET /api/events`, `GET /api/commands`, `POST /api/command` and `POST /upload`. Commands are JSON such as `{"name":"image.select","value":0,"text":"top"}`. Other commands cover independent grid placement, styles, audio and UI drawers. `tools/mcp_debugger.py` is a stdio MCP bridge. Prefer these tools for automated upload/testing; do not use the file explorer.

## Data and limits

PNG/JPEG imports: 12 MB, 4096 pixels per axis, up to 100 saved images. Imports and preferences persist locally. Sheet and grid placement last for the session; spatial anchors are not implemented. No account, cloud service, advertising, analytics upload, pen tracking or physical-drawing recording is required. The original source art and provenance are preserved in `art/`; AI illustrations are reprocessed with `python3 tools/process_reference_library.py` (Pillow and NumPy), preserving source files and recording hashes and alpha checks in `art/provenance/filtered-library.json`. The paired library is generated from original Blender geometry by `tools/blender/build_reference_pairs.py`.

Development diagnostics sample twice per second and retain up to seven sessions capped at approximately 16 MB each. They may include reference names and head/hand poses, so review them before sharing. Production policy/contact details and actual headset acceptance are tracked separately from compilation and package checks.

Full local delivery rebuild: `python3 tools/build_delivery.py`. Each Mac/release-candidate/review build runs in a fresh Unity process. The helper archives only identical, unreferenced stale XR simulation settings outside Assets before each build; it stops if settings differ or are referenced. Logs are `builds/delivery-mac.log`, `delivery-candidate.log`, and `delivery-review.log`.

Linux and Steam Frame delivery: see [builds, installation and validation limits](docs/steam-frame-and-flatpak.md). The headset uses the ARM64 OpenXR APK; the Flatpak contains the x86-64 desktop studio.

The [development plugin guide](docs/development-plugin.md) describes the local router, domain skills and the versioned source snapshot included for recovery.
