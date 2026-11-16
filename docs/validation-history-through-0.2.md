# Validation — 2026-10-11

## Candidate

- Artifact: `builds/SoftStrangeDrawing.apk`, 58,758,980 bytes (56.0 MiB).
- SHA-256: `40ebe586b136add9846f5a3bb6070c2752ae0b49e6152fc73c7f07fad505396c`.
- Identity: `com.softstrange.drawing`, version 0.1.0, version code 1.
- Unity: 6000.3.11f1, IL2CPP, ARM64 only, Vulkan, single-pass instanced stereo, development build.
- Android: minimum API 32, target/compile API 36 from the installed SDK.
- Signing: apksigner verification passed; Android Debug certificate. Suitable for local testing, not a store submission.

The current candidate includes stripping/UI-anchor repairs and passed `builds/build-fix.log`. Earlier runtime evidence below belongs to the initial APK (`21857fdd96cca40281f262af5e1173ffe27c43efd2da6c878a6f18f18e77283f`), unless explicitly marked as a corrected-build check.

## Passed

- Unity imported and compiled the application and editor scripts.
- OpenXR build validation passed with Meta Quest support, AR session, AR camera passthrough, XR Hands hand tracking, composition layers, and the required interaction profile enabled.
- Android native compilation and Gradle packaging completed successfully (`builds/build.log`: Build Finished, Result: Success).
- Generated scene has camera tracking actions, transparent camera background, XR origin, AR session, AR camera manager, reference/hand materials, and a non-null TMP font reference.
- APK inspection confirmed the package/version, `arm64-v8a`, Quest VR launch category, head-tracking feature, hand-tracking permission/feature, and required passthrough feature.
- Meta's camera provider also inserts Android/Meta camera permissions. This implementation displays compositor passthrough and does not request or process camera pixel frames.
- Real application startup in Unity Editor Play Mode, without a graphics device, initialized the controls, sample image, image library, and upload server.
- The running app returned HTTP 200 for the uploader page, HTTP 413 for an undersized payload, HTTP 415 for non-image data, and HTTP 202 for a transparent 64x64 PNG.
- Application log confirmed `SoftStrange: image received 64x64`, proving decode/save/display assignment beyond transfer acknowledgement. This was a desktop Editor run, not the Android player.
- Python helper and browser JavaScript syntax checks passed. `git diff --check` passed. The device helper correctly reports an absent device without attempting installation.

## Quest Pro installation and startup

A later connection check detected authorized Quest Pro `230YC01D7Z00N6` (Android API 34). The initial APK identified above installed successfully with `adb install -r`, preserving app data, and launched successfully. Android activity/window inspection confirmed the app was resumed and focused. Its running USB-forwarded upload page returned HTTP 200. Device logs confirmed `subsystem=True` for XR Hands; both hands were untracked in the captured sample. Runtime logs are in `builds/quest-runtime.log`.

## Not yet verified

Physical passthrough appearance, actual tracked-hand gestures, and drawing alignment still require a headset walkthrough. Installation and application focus do not prove these interactions.

Desktop Editor startup correctly reports that the Quest AR camera/session and hand subsystems are unavailable on this platform. Unity Editor Search also emitted an indexing exception; its stack was entirely in UnityEditor.Search, and the app remained running and accepted the upload. Headless startup does not prove UI rendering or shader appearance.

Required physical checks:

1. Accept USB debugging and confirm the ADB device model is a Quest.
2. Run `python3 tools/headset.py install`, prove launch/focus, inspect device logs.
3. Confirm stereo passthrough and the reference appearance with head movement.
4. Test each hand's midpoint pinch, wrist rotation, two-hand resize/turn, release, lost tracking, and pause/resume.
5. Upload PNG and JPEG through the browser, confirm decode on the Android player, image switching, and outline/opacity controls.
6. Align to real paper, lock, draw with a physical pen, and check comfortable legibility and registration.
7. Check UI pointing/direct pinch, hand skeleton display, hide/show, Bring here, and drawing with controls folded away.

Images persist across app restarts. This milestone deliberately keeps placement in the current tracking session; cross-session physical registration requires spatial anchors in a later milestone.

## Next action

Wear the connected Quest Pro, enable hand tracking, and confirm passthrough/reference appearance and the physical interaction checklist. The APK is installed and running; headset interaction acceptance remains open.

## Follow-up: reported return to home

The user reported the app closing/returning home. Inspection of the saved PID-specific startup log found 52 missing `SphereCollider` errors from runtime hand-joint primitive creation. This was a real engine-stripping bug that the earlier installation check missed. OpenXR still entered FOCUSED, initialized passthrough, and created both hand trackers.

A fresh baseline run of the originally installed build remained alive and focused across a 30-second observation window. Its uploader stayed available after startup. A recovered PID log showed both hands tracked repeatedly. The stereo screenshot (`builds/adb-validation-20261011-211124/headset.png`) visibly showed passthrough and tracked hand bones, but some labels were offset outside their buttons. Android exit history recorded the first process ending with OTHER KILLS BY SYSTEM / destroyTimeout; no native crash was proven. A force-stop record from the validation helper is an intentional restart, not evidence of the reported failure.

The initial helper's timestamp formatting caused logcat's time filter to fail. Those log checks are invalid in that first report; the separate `builds/baseline-recovered.log` recovered the actual PID evidence. The helper now uses device epoch timestamps, explicit device identity/hash checks, bounded timeouts, and Unity's bundled ADB.

Repairs: preserve the dynamically created SphereCollider/MeshCollider/MeshFilter/MeshRenderer types in `Assets/SoftStrange/link.xml`; reference collider types explicitly; center generated button/text anchors and pivots. Unity rebuilt successfully, and IL2CPP output contains SphereCollider metadata. Corrected-build installation and the final device loop are pending reconnection: USB dropped again after the build.
