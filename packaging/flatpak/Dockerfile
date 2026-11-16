FROM debian:bookworm
RUN apt-get update && apt-get install -y --no-install-recommends flatpak flatpak-builder ostree dbus ca-certificates xz-utils elfutils appstream-util python3 && rm -rf /var/lib/apt/lists/*
WORKDIR /workspace
