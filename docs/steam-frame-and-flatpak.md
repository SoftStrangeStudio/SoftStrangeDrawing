# Steam Frame and Linux Flatpak delivery

Built 2026-10-12. These are separate targets: the Steam Frame headset build is an ARM64 Android/OpenXR APK; the Flatpak is the x86-64 desktop studio with mouse/keyboard input and the house environment. The desktop Flatpak is not an immersive Frame port.

## Artifacts

| Artifact | Bytes | SHA-256 |
| --- | ---: | --- |
| Steam Frame APK, `builds/SoftStrangeDrawing-SteamFrame.apk` | 82,546,398 | `e2b10d0d9419a0f4796f435249b3b706bfbdc789536711b66be321f04df8d9eb` |
| Linux Flatpak, `builds/SoftStrangeDrawing.flatpak` | 34,087,808 | `9ec8189593248b7617f6de3488709b065edf5b9b0609b8bd06e673406e311d30` |

## Steam Frame configuration

Following [Valve's Unity instructions](https://partner.steamgames.com/doc/steamhardware/steamframe/engines/unity): Android minimum API 29 / target 36, ARM64, IL2CPP, Vulkan and built-in OpenXR. Valve OpenXR Utilities 1.0.9 is pinned to source commit `329c81f5a97a7f9e7740cf4307f1bfa9ce090b3a`; its license is bundled. Frame Controller Profile, Oculus Touch compatibility, Lepton validation, generic hand tracking and composition support are enabled. Meta/Quest XR features are disabled for this target and Quest settings are restored afterward. The target scene omits ARSession and ARCameraManager, retains tracked head pose, and uses a transparent camera clear. A dedicated OpenXR feature requests core AlphaBlend composition and logs the runtime's accepted blend mode. This is a request, not a physical passthrough acceptance result.

Tracked controllers take precedence over runtime skeletal poses in the Frame scene. Trigger selects controls, grip places the reference, two grips resize it. Freehand wrist-based pinch remains available only with a working runtime hand provider. Controller finger estimation must not be represented as proof of camera-based freehand tracking.

Unity/OpenXR/Valve validation and build passed. Package checks passed ARM64-only, v2 signing, size, minimum API, launchable activity, OpenXR permission and removal of Meta manifest dependencies/raw camera permissions. This is a development build with a test key and a loopback developer endpoint. No Steam Frame was connected; both-eye rendering, controller mappings, alpha passthrough, real-world alignment, lifecycle and performance remain untested.

## Deploy to Frame

Enable Developer Mode and launch **Lepton Development** on the headset. [Valve's ADB guide](https://partner.steamgames.com/doc/steamhardware/steamframe/adb_lepton) explains network and USB connection.

```sh
python3 tools/steam_frame.py FRAME_HOST_OR_IP --logs
```

The helper selects only that endpoint, rejects Quest/non-ARM64 targets, preserves app data with `install -r`, launches the activity and forwards host port 8791 to app port 8787. Inspect `http://localhost:8791/api/state` and upload reference bytes with POST `/upload`. Use the explicit IP if the default hostname `frame` does not resolve.

## Linux Flatpak

Built with Freedesktop Platform/SDK 26.08, x86-64, stable branch. Includes desktop/AppStream metadata, launcher icon, graphics and PulseAudio access. Network sharing allows the existing loopback upload/debug API. No broad host/home filesystem permission is granted. Images and preferences live in Flatpak's application-specific storage.

```sh
flatpak install --user ./SoftStrangeDrawing.flatpak
flatpak run com.softstrange.Drawing
```

The bundle declares its Flathub runtime repository; runtime dependencies are downloaded separately. [Flatpak bundle documentation](https://docs.flatpak.org/en/latest/single-file-bundles.html).

Export, bundle installation, metadata, architecture and permissions were checked in the local Colima Linux VM. AppStream validation passed. The software OpenGL launch initialized Mesa llvmpipe, then Unity Mono aborted with an amd64 relative-offset assertion under QEMU x86-on-ARM. An interpreter attempt also aborted; a constrained-address-space attempt did not start. No interactive Linux runtime acceptance or audio playback is claimed. Logs are `builds/flatpak-linux-runtime.log` and `builds/flatpak-linux-interp-runtime.log`. Native x86-64 Linux testing is required; this Flatpak is not qualified for Steam Frame's ARM64 immersive runtime.

## Rebuild

Build `SoftStrange.Editor.ProjectBuilder.BuildLinux` with the installed Unity Linux module, then run `bash tools/build-flatpak.sh`. On macOS the helper uses a Docker/Colima Linux builder and caches runtimes in its own named volume. AppStream composition runs on the build host to avoid SDK-tool mismatches. Build `SoftStrange.Editor.ProjectBuilder.BuildSteamFrame` for the headset APK. See `builds/steam-frame-flatpak-validation.json` for the exact delivered hashes and evidence boundaries.
