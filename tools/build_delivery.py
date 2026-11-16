#!/usr/bin/env python3
"""Build each player in a fresh Unity process, preserving stale simulation backups."""
import datetime
import pathlib
import shutil
import subprocess
from headset import UNITY, ROOT

def archive_stale_simulation_settings():
    archive = ROOT / 'builds' / ('xr-settings-backup-' + datetime.datetime.now().strftime('%Y%m%d-%H%M%S'))
    pairs = [
        ('Assets/XR/Temp/XRSimulationPreferences.asset','Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset'),
        ('Assets/XR/Temp/XRSimulationRuntimeSettings.asset','Assets/XR/Resources/XRSimulationRuntimeSettings.asset'),
        ('Assets/XR/Editor/XRSimulationRuntimeSettings.asset','Assets/XR/Resources/XRSimulationRuntimeSettings.asset'),
    ]
    for duplicate, canonical in pairs:
        source, target = ROOT / duplicate, ROOT / canonical
        if not source.exists() or not target.exists():
            continue
        if source.read_bytes() != target.read_bytes():
            raise RuntimeError(f'Simulation settings differ; preserve and inspect {duplicate} and {canonical}.')
        meta = source.with_suffix('.asset.meta')
        guid = next(line.split(':',1)[1].strip() for line in meta.read_text().splitlines() if line.startswith('guid:'))
        refs = subprocess.run(['rg','-l',guid,'Assets','ProjectSettings','--glob','!*.meta'],cwd=ROOT,capture_output=True,text=True)
        if refs.returncode != 1:
            raise RuntimeError(f'Cannot archive a referenced duplicate: {duplicate}: {refs.stdout}{refs.stderr}')
        for file in (source,meta):
            destination = archive / file.relative_to(ROOT)
            destination.parent.mkdir(parents=True,exist_ok=True)
            shutil.move(str(file),destination)
        print('Archived identical, unreferenced duplicate:',duplicate,flush=True)

def main():
    # Unity's simulation settings are temporarily moved during a build. A fresh
    # process prevents singleton state from leaking across consecutive targets.
    for name,target,method in [('mac','StandaloneOSX','BuildMac'),('candidate','Android','BuildReleaseCandidate'),('review','Android','Build')]:
        archive_stale_simulation_settings()
        log=ROOT/'builds'/f'delivery-{name}.log'
        subprocess.run([UNITY,'-batchmode','-quit','-projectPath',str(ROOT),'-buildTarget',target,'-executeMethod','SoftStrange.Editor.ProjectBuilder.'+method,'-logFile',str(log)],cwd=ROOT,check=True)
        print('Built',name,'—',log,flush=True)

if __name__ == '__main__':
    main()
