# Validation — cozy 0.3.0 candidate

## 2026-11-16 source synchronization and handoff

- GitHub target authorized by the user: SoftStrangeStudio/SoftStrangeDrawing, main. Local/remote baseline before synchronization was 31582aa; fetch showed no divergence. The source commit carries this record and docs/handoff.md; use its Git revision rather than a self-referential copied hash.
- Staged 963 source/asset/tool/doc files, approximately 111.6 MB before Git compression. Build outputs, Library/Logs, Flatpak cache, temporary Unity files, Python bytecode and signing credentials are excluded. Existing app source/assets/tools/package/project-setting hashes were checked against the pre-sync snapshot and remained unchanged.
- Added a verified 64-file plugin recovery ZIP and SHA-256 manifest under packaging/codex. The maintained canonical plugin source and private signing identity remain outside the repository.
- Python syntax checks passed for the core build/package/signing/MCP helpers. Plugin structural/source-owner checks passed. All 13 checks passed again for the unchanged release APK, SHA-256 8db3cbe81384450ddfa2b15234564f13e451d9dafb3626983d63a98f2ba7cb61.
- Whitespace checks passed for the coordinated Markdown, ignore file and Python helpers. A whole-initial-import whitespace check reports existing Unity serialized empty fields and generated PDF formatting; those assets were preserved rather than rewritten during synchronization.
- Reconciled stale README/readiness/checklist statements about version, wrist aim and resolved release signing. The minimal UI, final physical Quest acceptance and Meta submission remain pending.
- Drive task/handoff writes remain on hold at the user's explicit request while another agent sets up the project under the Luminary shared drive's Luminary-Projects. No external project/record or duplicate tracker was created. See docs/handoff.md for the verified old-folder and intended-home distinction.
- No fresh Unity build, new headset test, app-data removal, credential change, store submission or publication occurred during source synchronization.

## 2026-11-03 local development plugin

Installed/enabled soft-strange-drawing-kit@personal v0.1.1 using the supported Codex plugin installer. Canonical source: /Users/angelberger/plugins/soft-strange-drawing-kit. Main router plus 27 domain skills passed validation; all 22 tool dry-runs, three source-owner routes and four negative guards passed. All 64 installed files match source, and structural check also passed from the installed copy. Existing APK inspection through the helper passed all 13 package checks on the unchanged signed 0.3.1 artifact. No new application build, device installation/acceptance, public upload or store submission occurred. See docs/development-plugin.md and builds/plugin-validation-20261103.json for details.

## 2026-10-20 Quest release signing and build cleanup

- Final install artifact: `builds/SoftStrangeDrawing-QuestRelease.apk`, 62,946,522 bytes; SHA-256 `8db3cbe81384450ddfa2b15234564f13e451d9dafb3626983d63a98f2ba7cb61`.
- Version 0.3.1, Android version code 4, package `com.softstrange.drawing`; Quest scene, ARM64 IL2CPP/Vulkan, minimum SDK 32, explicit target SDK 34, non-development build including the current wrist-forward pointer.
- Automatic install location and launcher exclusion from recents now survive manifest merging. The developer upload endpoint and automatic state recording remain disabled in release builds; no Internet, raw-camera or microphone permissions in the APK.
- Created the dedicated RSA-3072 release identity with user authorization, outside the repository. Certificate SHA-256: `2f9bd5fe932f708f0d35f11179fae190d9f6263460528d6b9648f5658e13b25e`. The private signing folder is owner-only (0700), key and password metadata are 0600. Back up the entire private signing folder securely; never commit its contents.
- APK signature verification passed v3 at the APK's minimum SDK and v2 with its independent API 24-27 verification range. Corrected preflight to verify v2 independently because default apksigner verification selects v3 for minimum SDK 32. Explicit v2/v3 signing enabled. zipalign 16 KB native-library alignment check passed.
- All 13 APK preflight checks passed. Evidence: `builds/package-checks/SoftStrangeDrawing-QuestRelease.json`, its manifest/signature/badging companions, and `builds/build-release.log`. Python syntax checks passed for the edited tooling; Unity build succeeded.
- Repeat build: `python3 tools/headset.py build --release`, which builds the intermediate candidate and signs `SoftStrangeDrawing-QuestRelease.apk` using the existing private identity. First-time identity creation is explicit through `python3 tools/sign_release.py --create`; existing keys are never replaced.
- This new release signature differs from earlier Android test signing. Android cannot update a test-signed installation in place. Uninstalling the old app deletes local app data and requires explicit approval; no uninstall or device installation was performed here.
- The previous candidate was preserved before rebuilding. An unrelated Unity 2020 licensing client initially blocked startup; the build CLI now isolates the licensing connection and the retry succeeded without closing that project.
- Package validation is not hardware or store acceptance. This signed build still needs a complete physical Quest session, final publisher/privacy/listing information and Meta review.

This record concerns version 0.3.0 / Android version code 3. Previous evidence is retained in [validation history](validation-history-through-0.2.md). Store qualification remains incomplete; see [readiness](store/readiness.md).

## Artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Review APK, `builds/SoftStrangeDrawing.apk` | 90,093,498 | `789450e99efdf1250b0b432be0be5c858346ee8d39199969bc9c51bbb048d9a5` |
| Non-development APK, `builds/SoftStrangeDrawing-ReleaseCandidate.apk` | 62,843,543 | `0eb5c19760d26cab7aeb902ff6fea05564ed42433f5ee7ae996314bf9d2f7c11` |

Both are ARM64 IL2CPP, Android minimum API 32 / target API 36, with APK signature scheme v2 verified. Both are signed with the **Android test key**. The candidate is non-debuggable and omits Internet/raw-camera/microphone permissions. The review APK retains Internet permission for its loopback developer endpoint. Both have the VR launch category, focus-aware metadata, required passthrough feature and contextual passthrough startup metadata.

Reproduce package inspection: `python3 tools/apk_preflight.py builds/SoftStrangeDrawing.apk builds/SoftStrangeDrawing-ReleaseCandidate.apk`. Detailed manifests, signature reports and JSON checks are under `builds/package-checks/`.

## Desktop and asset checks

- Unity Mac compilation and player startup passed. The home screen and side/top gallery were visually inspected and clicked in the native player.
- Fourteen API/state assertions passed: side/top selection, grid independence and persistence through lock/recenter/image change, automatic dotted Bezier conversion, width/color/opacity, mute, one active drawer, grid placement from Home and upright menu rotation. Evidence: `builds/cozy-functional-validation.json`.
- All 120 paired PNGs passed white-RGB, alpha/background and no-clipping checks. Evidence: `docs/paired-library-validation.json`, `builds/paired-library-final-review.jpg`.
- All four original audio files decoded with expected durations and peaks below 0.16 full scale. This is waveform validation, not human audio-comfort acceptance.
- API upload on the Mac delivery player returned 202 and then confirmed decode, persistence, selection and visible drawing state. Imported thumbnails were visually inspected. Evidence: `builds/cozy-api-upload-validation.json`.
- Native Mac image chooser opened, but automated selection did not complete. No chooser-based import success is claimed. Per user preference, subsequent automation uses upload/API tooling and avoids the file explorer.
- Python helpers compiled; `git diff --check` passed.

## Quest Pro review APK

Earlier review APK `fca35231375583d9db6bcfb0b2ea90f1ce7f20327bcfc2d2255c5b8ba69654ee` was installed on authorized Quest Pro `230YC01D7Z00N6`. The installation used `adb install -r`, preserving app storage. `builds/adb-validation-20261011-235246/report.json` matched that earlier installed SHA-256 to the then-current local artifact and observed the same process for a 30-second window.

Passthrough-layer creation and XR hand-subsystem startup were logged. Active application samples were 71.7–72.0 FPS. The headset idled after about 14 seconds; the final state correctly changed to `inputAllowed=false`, with logs confirming audio pause. No tracked hands were visible and no valid headset screenshot was captured in this idle run. This demonstrates startup and lifecycle behavior, not physical gesture acceptance or sustained frame-rate qualification.

No crash, fatal exception or missing-component creation failure was captured. There were two SDK error-level capability messages: `No suitable capture camera found` and `Failed to look up xrDiscoverSpacesMETA`. A missing-script warning pair also remains in package startup logs. These require follow-up and are not represented as clean logs.

## Required acceptance still open

Real stereo appearance, comfortable ray/pinch interaction, two-hand rotation/scale, controller switching, tracing on paper, interrupted-input recovery, native Quest file import, recenter behavior under the OS, sustained performance/thermal load, and additional declared headset families. Public support/privacy information, production signing, store media/metadata and Meta review are also outstanding.

## Non-development candidate device attempt

This earlier attempt used SHA-256 `b2882b8231ca1158e271585d79606087aa182fc8085fdd1ee167fefc70cea960`, before the latest art/menu updates. `builds/cozy-release-device/report.json` confirms install and matching SHA-256, with no captured fatal exception. The headset lost focus before application startup completed, so this attempt did not qualify rendering or passthrough. The endpoint did not respond, but that alone is not a runtime proof of its configured disabled state. The review APK was subsequently restored and launched for user testing.

## Filtered AI library update

All 60 preserved AI illustrations are again accessible in the default AI art library. The 60 paired model references retain their own Side / top category. The filter removes low-alpha haze and tiny disconnected specks, converts broad dark fill to a boundary, strengthens fine strokes, and pads each image. It does not replace the artistically generated contour or remove all intentional hatching. All output RGB is pure white; interiors and backgrounds remain transparent. Source originals are unchanged; settings, hashes and per-image checks are in `art/provenance/filtered-library.json`. All 60 passed visible-ink, white-RGB, clear-border and majority-transparency checks.

Native Mac gallery inspection covered AI art, Side / top and My images; imported thin-stroke thumbnails were visible. All 60 AI illustrations loaded through the runtime API. All 60 also converted to curves without truncation (maximum 648 paths); dense turtle solid/dotted rendering was visually checked. The bounded path budget is now 2,048; unusually dense imports fall back to the full solid reference instead of displaying an incomplete drawing. Curve conversion verification is recorded in `builds/ai-library-runtime-validation.json`.

The current rebuilt APK is not covered by the earlier Quest evidence above. The final USB recheck found no device; final-build installation and physical gesture acceptance remain pending.

## Reproducible build cleanup

Identical, unreferenced stale XR simulation settings were archived with their metadata under `builds/xr-settings-backup-*`, preserving the canonical settings. `tools/build_delivery.py` runs each Unity target in its own process. The final Mac build and runtime have no missing-script, exception or simulation-asset-move messages in their captured logs; the older Quest logs above remain separate evidence. Final Mac Home was visually checked with the filtered AI shark, and updated dot defaults were confirmed via the runtime state API (`builds/delivery-mac-startup.json`).

All three isolated delivery builds completed successfully. Final Android build logs contain no simulation-asset-move warnings. Both final APKs passed ARM64/v2/size/VR/focus/passthrough and camera-permission checks; the non-development candidate also passed non-debuggable and no-Internet checks. Test-key signing is explicitly unresolved. Final `tools/headset.py install` could not find an ADB device, so neither final artifact is represented as installed or hardware-accepted.

## Wrist-only aim update

The latest review APK changes HandInput ray direction to the tracked wrist rotation’s forward axis, with a 75 ms exponential smoothing time constant. Origin smoothing remains 25 ms at the thumb/index midpoint. Controller aim retains 45 ms smoothing. Hand input now requires the wrist and pinch tips rather than unrelated palm/middle-tip poses; tracking loss and reserved-system-gesture cancellation remain active. README, interaction flow, acceptance checklist and repo memory were updated.

Unity Android compilation/build and APK preflight passed (`builds/wrist-aim-build.log`, `builds/package-checks/SoftStrangeDrawing.json`); `git diff --check` passed. ADB still listed no device and installation could not proceed. Stability, both-hand forward alignment and pinch/release comfort require the next physical headset check. The release candidate and Mac artifacts above precede this wrist-only update; only the review APK was rebuilt for this change.
