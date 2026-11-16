# Local development plugin

Canonical source: `/Users/angelberger/plugins/soft-strange-drawing-kit`.
Personal plugin identity: `soft-strange-drawing-kit@personal`, source version 0.1.1.
Entry skill: `skills/soft-strange-drawing-kit/SKILL.md` in that source directory.

The project AGENTS.md directs app work through the main router. One primary domain and affected neighbors are loaded per request. There are 27 subskills, covering product/architecture, hands/workspace/interface, references/imports/outlines, environment/feedback, preview/visual/functional/device/performance checks, build environment/platforms/builds/packaging/signing/release/store, and routing/contracts/context/documentation/handoff. These are local instructions, not independent agents or new runtime components.

The plugin uses existing Unity/ADB/app tools. The helper `scripts/kit.py` supports `inspect`, file-owner `route`, `doctor`, `check`, existing loopback `api` reads and `tool` command plans. Its 22 registered tool plans default to no execution; `tool --execute NAME` performs only the explicitly selected authorized operation. Device-targeted operations require the supplied serial/endpoint. No uninstall, publishing or background-update tool is provided. Tool plans expose side effects before execution.

Examples, run from the plugin directory:

- `python3 scripts/kit.py inspect interface`
- `python3 scripts/kit.py route Assets/SoftStrange/Runtime/HandInput.cs`
- `python3 scripts/kit.py doctor`
- `python3 scripts/kit.py tool build-release`
- `python3 scripts/kit.py check`

For an authorized build, `python3 scripts/kit.py tool --execute build-release` reuses `tools/headset.py build --release`. Signing uses the existing private identity, never a copied key/password. A successful plugin/helper validation is not a new application build or device acceptance.

Knowledge maintenance is part of app work after meaningful decisions and verified changes. Current project state stays in memory.md, docs/validation.md and docs/store/readiness.md. Methods and ownership live in the canonical plugin. Do not mirror state in the installed cache or require a formal request packet for ordinary feedback. A proposed UX idea is not marked implemented until the app changes and relevant checks run.

After changing plugin behavior, bump its manifest version, validate changed skills with the skill-creator validator, run helper check and relevant read-only/dry-run checks, then reinstall with `codex plugin add soft-strange-drawing-kit@personal --json`. Confirm the installed version and enabled state with `codex plugin list --json`. A fresh chat/reload may be needed for discovery; AGENTS.md still links the source in an existing chat. Do not hand-edit cache/config or guarantee hot reload.

This is a local personal plugin. No new MCP server, network listener, scheduled task, runtime package or GitHub change is part of installation.

## Installation and validation: 2026-11-03

Supported installer readback confirms version 0.1.1 installed/enabled under the personal marketplace. All 64 installed files match canonical source. The main router and all 27 domain skills passed skill-creator validation. All 22 tool plans were exercised without execution, three source-owner routes passed, and four negative checks rejected a missing device serial, a non-loopback API endpoint, an out-of-project path and forwarding --create to the normal signer.

The existing APK inspector was also run through the helper on the unchanged release APK and all 13 checks passed. This is not a fresh app build or hardware/store acceptance. The system interpreter lacks Pillow; image/store-art commands use the verified bundled Python with Pillow, NumPy, ReportLab and PDFium. The skill validator's PyYAML dependency was isolated in /tmp for validation rather than installed globally. Evidence is in builds/plugin-validation-20261103.json. No runtime source, app assets, private credentials, headset installation or GitHub records were changed by plugin setup.

## Versioned recovery snapshot — 2026-11-16

The source repository now includes `packaging/codex/soft-strange-drawing-kit-0.1.1.zip` and `plugin-snapshot.json`. This is a verified 64-file recovery snapshot of the maintained source, not a second active plugin tree. It contains no signing files, caches or app diagnostics. The manifest records its SHA-256. After changing/reinstalling the canonical plugin, refresh this labeled snapshot as part of an authorized source synchronization.

On recovery, inspect the archive, restore its top-level soft-strange-drawing-kit folder to the intended local plugin source location, and verify registry/project.json plus project source links for that machine. Use the supported personal-marketplace installer after confirming that location; do not edit installed caches. Absolute Unity/project/runtime paths are machine-specific. No credential restoration is supplied by this snapshot.
