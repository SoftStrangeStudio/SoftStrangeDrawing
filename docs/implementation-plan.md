# Implementation plan and intended outcome

## Summary

Build a local Quest app that places a reference image over the real environment, lets either hand position it, and locks it for tracing on a physical surface. Unity is the smallest implementation because the installed Android toolchain and existing OpenXR/Meta packages cover rendering, lifecycle, passthrough, and tracked joints. A custom Rust version would additionally need native Android packaging/lifecycle, graphics and compositor plumbing, Quest passthrough extensions, joint access, font/UI rendering, image decoding, and equivalent device debugging.

## Outcome

The user wears the headset and sees their room plus a transparent reference. A local browser sends a PNG/JPEG over USB to the running app. The upload is validated, decoded, saved, and displayed with its original aspect ratio. The user pinches near it to move it and turns their wrist to rotate; a second hand adds scale and axis rotation. A lock prevents drawing gestures from moving the image. Adjustable alpha makes paper and pen visible, while outline mode removes flat image regions.

A real headset walkthrough demonstrates: stereo passthrough, head tracking, both hands' joints, grab/release, wrist rotation, two-hand resize/rotation, tracking-loss recovery, upload without rebuilding, image switching, outline processing, and locking while tracing.

## Changes

```text
SoftStrangeDrawing/
|-- Assets/SoftStrange/
|   |-- Editor/ProjectBuilder.cs      Android settings, XR features, scene, build
|   |-- Runtime/
|   |   |-- DrawingApp.cs            startup, image library, controls, hand routing
|   |   |-- HandInput.cs             tracked poses, pinch midpoint, skeleton
|   |   |-- ReferenceSheet.cs        grab ownership and transform manipulation
|   |   |-- PinchButton.cs           physical pinch to uGUI callback
|   |   `-- UploadServer.cs          USB-forwarded HTTP upload, bounded input
|   |-- Resources/                  uploader page, materials
|   |-- Scenes/Drawing.unity         generated, inspectable entry scene
|   `-- Shaders/Reference.shader     transparency and Sobel edge extraction
|-- Assets/XR/                      generated serialized XR configuration
|-- Packages/                       pinned Unity package dependencies
|-- ProjectSettings/                Unity/Android project settings
|-- tools/headset.py                 build, install, connect, upload, status
|-- builds/                         ignored local APK and build log
`-- docs/                           plan and validation evidence
```

Implementation sequence: pin packages, configure Android/OpenXR, generate the rig and AR session, implement the image sheet and shader, add midpoint pinch manipulation and tracked skeleton, add local image upload/storage, add compact controls, compile/build, inspect APK, install and launch on an authorized Quest, perform headset interaction checks.

## Validation

Use Unity's actual import/compile and Android build, inspect APK identity/permissions/features/ABI and signing, verify ADB device authorization and model, install without clearing data, prove application focus, inspect runtime logs, and perform a physical interaction checklist. Do not infer headset acceptance from compilation or installation.

## Review and gates

The user authorized implementation, a test APK, and uploading to the connected headset for testing. This first version targets hands and USB upload. Cross-session spatial anchors, wireless/cloud upload, Rust implementation, automated surface mapping, and store distribution are separate future decisions. No paid services or production credentials are used.
