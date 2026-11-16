# Meta Quest submission readiness — 0.3.1 candidate

**Decision: review candidate, not approved for public store submission.**

This pass implements the cozy drawing experience and prepares reproducible APKs. A successful local build does not establish every Meta requirement. Use this record together with the exact artifact hashes and device evidence in `docs/validation.md`.

## Implemented in the candidate

- Calm transparent home, circular three-action drawing dock, contextual Ink/Grid/Studio drawers and clear placement/lock guidance.
- 60 cleaned AI illustrations, plus 60 model subjects with 120 matched side/top white transparent views and 60 original stylized 3D models. Original AI artwork remains preserved; derived filter outputs have reproducible settings and hashes.
- Original quiet ambient audio and interaction cues, mute/volume persistence, gentle model/drawer animation and reduced motion.
- Easier bounded pinch thresholds, smooth wrist-forward pointing, system-gesture suppression, controller trigger/grip fallback and tracking-loss cancellation.
- Upright menu frame with gentle yaw following, independent grid, automatic dotted/width curve conversion, color/opacity controls and native image import.
- Focus/pause handling cancels input and pauses audio. State publication on lifecycle events prevents stale active-state reporting.
- Android ARM64 IL2CPP, Vulkan, OpenXR, contextual passthrough loading background, local-only image storage, no raw-camera/microphone permission, backup disabled.
- Non-development candidate disables the local developer server and automatic pose recording. Review APK keeps bounded diagnostics and USB upload for device testing.
- App launcher icon, dependency notices, privacy-policy draft and store-description draft.

## Acceptance matrix

| Area | Evidence or remaining check |
| --- | --- |
| Packaging | Inspect both final APKs for version/code, ARM64, SDK declarations, signature v2, size, focus-aware VR category and required features. Save machine-readable results. |
| Permissions | Verify final merged APK manifests, not just project XML. Release candidate must omit Internet, camera and microphone permissions. Hand/OpenXR permissions remain. |
| Production identity | Dedicated release signing is complete for `SoftStrangeDrawing-QuestRelease.apk` 0.3.1 (4). Review/intermediate APKs retain test signing. Publisher organization/app entry and package ownership still require confirmation. |
| Startup and passthrough | Earlier 0.2 device evidence exists. 0.3 intermediate app launched on Quest Pro and reported ~72 FPS. Final review APK was installed and matched by hash; passthrough-layer creation and short ~72 FPS active samples were recorded. Headset idled after ~14 seconds, so final visual/gesture acceptance remains open. |
| Performance | **Pending full qualification:** sustained GPU/CPU/thermal measurements on each declared device, worst-case gallery, imported photo and dense dotted curves. Short FPS samples are only smoke evidence. |
| Input | Desktop UI/API checks cover library, grid, styles and mute. **Pending physical acceptance:** pinch comfort, two-hand rotation/scale, controller switching, system gesture and interrupted-grab recovery. |
| Lifecycle | Idle/pause log evidence shows input and audio suppression. **Pending:** repeated sleep/wake, headset removal, system dashboard, interruptions and return, boundary/tracking loss and OS recenter. |
| Import | PNG/JPEG bounded decode and corrupt-file fallback implemented. Desktop file chooser opened, but end-to-end chooser selection was not validated; user requested no file explorer. Use upload/API tests and manual headset import acceptance. |
| Privacy and policy | Policy draft matches current offline architecture. **Pending:** publisher contact, public policy URL, exact production-build review and Meta data-use declarations. |
| Store presentation | Launcher icon, description and vector brand artwork drafts exist in `art/store`: hero, landscape/square/portrait covers, mini cover, transparent logo and icon at the published sizes. **Pending:** publisher visual approval and real final Quest screenshots/video. Desktop house footage must be labeled desktop simulation. |
| Content/rights | Original procedural geometry/render/audio provenance and third-party license texts included. Publisher must review final redistribution/attribution requirements and filtered AI artwork and preserved originals. |
| Metadata | **Pending:** supported devices/input/play modes, pricing, age/content ratings, language/support information, developer verification and Meta submission/review. |
| Entitlement | No Platform entitlement integration. Meta's current VRC table labels this recommended, not required; decide once platform identity and purchase model are established. |

Known diagnostics: final Quest review logs contain two nonfatal Meta SDK capability errors (capture camera unavailable on Quest Pro and unavailable `xrDiscoverSpacesMETA`) and a missing-script warning pair. No crash was captured. These remain investigation items rather than a clean-log or full-qualification claim.

## Physical acceptance sequence

1. Cold-launch the exact APK in passthrough. Read Home and choose Side, then Top. Confirm both eyes render correctly and the model/menus are comfortable.
2. Point using the wrist’s forward direction. Keep the wrist steady and curl the fingers; the ray direction should remain steady. Slowly close thumb/index: ray should fade in smoothly, select once, and release reliably. Test both hands.
3. Pinch near the reference, move/rotate with one hand, resize with two, release one hand, lock, and trace on paper without accidental movement.
4. Show and move the grid. Lock/reference-change/recenter must leave it visible and independent. Finish grid placement before drawing.
5. Test solid/dotted strokes, width, opacity and white/black on different paper/lighting. Import a PNG and a photo; test cancellation and a bad file.
6. Open the system dashboard during a pinch. Audio and interaction must pause, hands/rays must hide, and return must require a fresh gesture. Repeat with tracking loss, controllers and sleep/wake.
7. Run a sustained drawing session, then inspect frame timing, memory and crash logs. Repeat on every device family claimed in the listing.

## Primary requirement sources

Reviewed during this implementation pass:
- [Meta Quest VRC guidelines](https://developers.meta.com/vr/resources/publish-quest-req/)
- [Release manifest requirements](https://developers.meta.com/vr/resources/publish-mobile-manifest/)
- [Contextual passthrough loading](https://developers.meta.com/vr/documentation/unity/unity-customize-passthrough-loading-screens/)
- [Safety and privacy](https://developers.meta.com/vr/design/safetyprivacy/)

No store submission or publication was performed. Authorized local release signing on 2026-10-20 used the dedicated private identity; its credentials remain outside this repository.

## Current UX and coordination gate — 2026-11-16

The current menus were reviewed in the Mac player. The agreed minimal interaction-driven redesign remains unimplemented: subtle reference affordances, deliberate lock/edit and contextual controls replace most persistent UI. Keep this separate from already implemented features and signed-package evidence. Drive task/handoff writes are on hold at the user's request while another agent establishes the project in the Luminary shared drive's Luminary-Projects. See docs/handoff.md and docs/completion-checklist.md for continuation.

## 2026-10-20 release candidate update

`builds/SoftStrangeDrawing-QuestRelease.apk` is version 0.3.1 (4), non-development, target SDK 34, and signed with the dedicated release identity. All 13 package checks and the 16 KB zip alignment check passed. See `docs/validation.md` for hashes, signing certificate fingerprint and build evidence. This supersedes test-key signing and automatic SDK selection as blockers for this artifact. Hardware walkthrough, publisher/listing/privacy completion and Meta review remain pending. The private signing directory must be backed up securely; it is outside the repository.
