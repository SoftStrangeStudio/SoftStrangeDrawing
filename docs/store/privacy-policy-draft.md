# Soft Strange Drawing privacy policy — publication draft

Operator: Soft Strange. Effective date: set when published.
Public contact email: **pending publisher input**.
Public policy URL: **pending publisher input and publication**.

Soft Strange Drawing lets you place reference outlines over your surroundings to draw on physical paper. The app works offline, without an app account, advertising, an analytics service, or a cloud image library.

## Information processed on your device

When you choose an image through the system file picker, the app stores a copy in its private application storage. The app does not read your other documents. Imported images and drawing preferences remain on that device. Application backup is disabled in the Android manifest. Uninstalling the app removes its private local storage; exported files you copied elsewhere remain where you saved them.

The app receives headset, controller and hand tracking poses from the XR runtime to position your workspace and interpret pointing and pinching. It does not identify you from your hands. The headset operating system composites passthrough behind the app; the app does not request raw camera or microphone access and does not receive camera images.

The non-development release candidate does not start the local upload/debug server and does not automatically record pose diagnostics. Development review builds can record local frame rate, application commands, reference names, tracking poses and errors for troubleshooting. They retain at most seven diagnostic sessions, capped at approximately 16 MB each. These files are not uploaded automatically. If you choose to share a diagnostic file with support, inspect its contents first. The developer endpoint binds only to the device loopback address and requires a local connection or USB ADB forwarding.

The app has no in-app purchases, advertising identifiers, third-party analytics or developer-operated backend. Meta Horizon OS and its system services are governed by Meta's own policies; this document describes the application's behavior.

## Your choices

You may use the bundled library without importing personal images. Sound and decorative motion can be adjusted in the Studio drawer. You can remove local app data through the headset's application-management controls or by uninstalling the app. Contact the publisher at the public email above once this policy is published for questions about this app.

This draft must be completed, reviewed against the exact production build and published at an accessible public URL before store submission. It is not a claim of legal or store approval.
