# Completion checklist

## Current continuation state — 2026-11-16

- [x] Dedicated release-signed Quest APK 0.3.1 (4); package checks passed. See docs/validation.md for exact artifact identity and evidence scope.
- [x] Local development plugin v0.1.1 with 27 domain subskills and 22 tool plans, plus the main router.
- [x] Current-source Mac UX review and captures; findings summarized in docs/handoff.md.
- [ ] Implement the agreed minimal interaction-driven UI: direct outline manipulation, subtle affordances, deliberate lock/edit and contextual adjustments.
- [ ] Improve reference browsing and paper-alignment guidance, preserving the independent grid and advanced outline controls.
- [ ] Complete native Quest import/cancel/recovery acceptance and define safe per-image removal.
- [ ] Complete final-build physical Quest, lifecycle, sustained performance and supported-device acceptance.
- [ ] Complete publisher identity, public support/privacy, rights review, final Quest media, ratings/metadata and Meta review.
- [ ] Sync Drive tasks/handoff only after the other agent finishes setting up the owning project in the Luminary shared drive's Luminary-Projects. Do not write to the older My Drive project or create a duplicate.

The following sections retain the earlier 0.3.0 implementation/acceptance history. They do not mean the proposed minimal UI is implemented. The current handoff is docs/handoff.md.

## Implemented

- [x] Quest/OpenXR MR foundation, tracked joints and controller fallback.
- [x] Easier pinch, gradual ray opacity, smoothed wrist-forward aim, gesture/focus ownership.
- [x] Cozy transparent home, circular drawing dock, contextual drawers and semantic vector icons.
- [x] Upright gently following menu and independently movable persistent-visible session grid.
- [x] 60 cleaned AI illustrations plus 60 subjects with 120 matched side/top transparent white views and 60 original stylized 3D model previews.
- [x] Original audio cues/ambient loop, gentle animations, mute/volume and reduced motion.
- [x] Automatic cubic-Bezier conversion, dotted strokes, line width/color/opacity and photo edge controls.
- [x] Local image library, bounded import, thumbnail handling and recovery from an unavailable saved image.
- [x] Service/public command architecture, bounded diagnostics, MCP bridge and improved ADB validation.
- [x] Contextual passthrough loading metadata, minimal camera permissions, backup disabled, candidate/debug build separation.
- [x] Launcher icon, bundled dependency notices, privacy and listing drafts, submission acceptance checklist.
- [x] Desktop UI and 14 API/state assertions passed; all 180 reference PNGs passed alpha/white/clipping audits; all 60 filtered AI images loaded and converted without path truncation.
- [x] Earlier 0.3 review APK installed without clearing app data; identity matched, passthrough initialized, short active samples ~72 FPS, no crash in 30-second observation. Latest filtered-art rebuild still needs installation after USB reconnect.

## Remaining acceptance / publisher actions

- [ ] Physical final-build headset walkthrough: both hands, pinch/rotate/scale comfort, controllers, import, real-world drawing alignment and stereo appearance. Headset idled before hands were captured.
- [ ] Sustained performance and lifecycle testing on each declared headset, including worst-case imported curves and sleep/system interruptions.
- [ ] Investigate two nonfatal Meta SDK capability errors (capture camera unavailable on Quest Pro, unavailable spatial-discovery entry point).
- [x] Complete dedicated release signing for the 0.3.1 Quest release artifact (supersedes the earlier 0.3.0 signing gap).
- [ ] Complete support contact, public privacy policy, store media/ratings/metadata, developer verification and Meta review.
- [ ] Complete native file-picker selection manually; automated testing uses upload/API tools per user preference.

## Earlier broader platform work remains separate

- [ ] Genuine tracked-hand simulation driving the real interaction pipeline; current house replay directly transforms the sheet.
- [x] Linux player and Flatpak exported, metadata validated, bundle installed in Linux build container.
- [ ] Native x86 Linux interactive acceptance; QEMU/ARM launch aborts in Mono.
- [x] Steam Frame ARM64 APK built with Valve controller profile, Lepton validation, no Meta dependencies and core alpha-blend request.
- [ ] Steam Frame physical passthrough/input/lifecycle/performance acceptance.

A candidate is not a store approval. See `docs/store/readiness.md` and `docs/validation.md` for evidence and limits.
