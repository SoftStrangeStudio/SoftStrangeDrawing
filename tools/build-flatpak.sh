#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if ! command -v flatpak-builder >/dev/null; then
  command -v docker >/dev/null || { echo "Install Flatpak tools on Linux, or start Docker/Colima on macOS." >&2; exit 1; }
  docker build -t softstrange-flatpak-builder packaging/flatpak
  # The builder runs inside the Linux VM; bubblewrap needs nested namespaces.
  docker run --rm --privileged -v "$PWD:/workspace" -v softstrange-flatpak-cache:/var/lib/flatpak softstrange-flatpak-builder bash tools/build-flatpak.sh
  exit
fi
test -f builds/Linux/SoftStrangeDrawing.x86_64 || { echo "Build the Linux player with ProjectBuilder.BuildLinux first." >&2; exit 1; }
manifest=packaging/flatpak/com.softstrange.Drawing.json
runtime_version=$(python3 -c 'import json; print(json.load(open("packaging/flatpak/com.softstrange.Drawing.json"))["runtime-version"])')
flatpak remote-add --if-not-exists flathub https://dl.flathub.org/repo/flathub.flatpakrepo
flatpak install -y --noninteractive --arch=x86_64 flathub "org.freedesktop.Platform//$runtime_version" "org.freedesktop.Sdk//$runtime_version"
flatpak-builder --arch=x86_64 --default-branch=stable --disable-rofiles-fuse --force-clean --repo=builds/flatpak-repo builds/flatpak-build "$manifest"
# Compose metadata on the build host rather than relying on a tool inside the SDK.
appstream-compose --basename=com.softstrange.Drawing --prefix="$PWD/builds/flatpak-build/files" --origin=flatpak com.softstrange.Drawing
flatpak build-export --arch=x86_64 --exclude='lib/debug/*' builds/flatpak-repo builds/flatpak-build stable
flatpak build-bundle --arch=x86_64 builds/flatpak-repo builds/SoftStrangeDrawing.flatpak com.softstrange.Drawing stable --runtime-repo=https://dl.flathub.org/repo/flathub.flatpakrepo
