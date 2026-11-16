#!/usr/bin/env python3
"""Build, install, connect the browser uploader, or send an image over USB."""
import argparse
import datetime
import json
import re
import time
import hashlib
import pathlib
import subprocess
import sys
import urllib.request
import webbrowser

ROOT = pathlib.Path(__file__).resolve().parent.parent
PACKAGE = 'com.softstrange.drawing'
UNITY = '/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity'
ADB = str(pathlib.Path(UNITY).parents[3] / 'PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb')


def run(*args, timeout=15):
    if args[0] == 'adb':
        args = (ADB, *args[1:])
    return subprocess.check_output(args, text=True, timeout=timeout).strip()


def device(serial):
    rows = [line.split() for line in run('adb', 'devices').splitlines()[1:] if line.strip()]
    if serial:
        rows = [row for row in rows if row[0] == serial]
    ready = [row[0] for row in rows if row[1] == 'device']
    if len(ready) != 1:
        raise RuntimeError('Connect one Quest with Developer Mode enabled and accept USB debugging in the headset. Use --serial if multiple devices are connected.\n' + run('adb', 'devices', '-l'))
    return ready[0]



def validate(adb, serial, model, duration, port=8790):
    """Restart only this app, then capture startup and observe process/focus/XR state."""
    stamp = datetime.datetime.now().strftime('%Y%m%d-%H%M%S')
    folder = ROOT / 'builds' / ('adb-validation-' + stamp)
    folder.mkdir(parents=True)
    report = {'serial': serial, 'model': model, 'duration': duration, 'samples': [], 'checks': {}}

    def capture(name, *command, timeout=15):
        try:
            result = run(*adb, *command, timeout=timeout)
            (folder / name).write_text(result + '\n')
            return result
        except (subprocess.CalledProcessError, subprocess.TimeoutExpired) as error:
            (folder / name).write_text(str(error) + '\n')
            return ''

    capture('package.txt', 'shell', 'dumpsys', 'package', PACKAGE)
    installed_path = capture('installed-path.txt', 'shell', 'pm', 'path', PACKAGE).splitlines()
    if installed_path:
        hash_output = capture('installed-sha256.txt', 'shell', 'sha256sum', installed_path[0].removeprefix('package:')).split()
        installed_hash = hash_output[0] if hash_output else ''
        local_hash = hashlib.sha256((ROOT / 'builds/SoftStrangeDrawing.apk').read_bytes()).hexdigest()
        report['installed_sha256'] = installed_hash
        report['local_sha256'] = local_hash
        report['checks']['installed_apk_matches_local'] = installed_hash == local_hash
    capture('device-api.txt', 'shell', 'getprop', 'ro.build.version.sdk')
    capture('exit-history-before.txt', 'shell', 'dumpsys', 'activity', 'exit-info', PACKAGE)
    # Timestamp filtering retains startup errors without clearing shared device logs.
    since = run(*adb, 'shell', 'date', '+%s') + '.000'
    run(*adb, 'shell', 'am', 'force-stop', PACKAGE)
    run(*adb, 'shell', 'am', 'start', '-W', '-n', PACKAGE + '/com.unity3d.player.UnityPlayerGameActivity')
    run(*adb, 'forward', f'tcp:{port}', 'tcp:8787')
    started = time.monotonic()
    while time.monotonic() - started < duration:
        try:
            connected = run(*adb, 'get-state', timeout=5) == 'device'
        except (subprocess.CalledProcessError, subprocess.TimeoutExpired):
            connected = False
        if not connected:
            report['checks']['disconnected'] = True
            (folder / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
            raise RuntimeError('Headset disconnected during validation. Partial evidence: ' + str(folder))
        pid = capture('latest-pid.txt', 'shell', 'pidof', PACKAGE)
        activity = capture('latest-activity.txt', 'shell', 'dumpsys', 'activity', 'activities')
        focused = any(PACKAGE in line for line in activity.splitlines() if 'mCurrentFocus=' in line or 'ResumedActivity:' in line)
        http = False
        state = {}
        try:
            with urllib.request.urlopen(f'http://localhost:{port}/api/state', timeout=3) as response:
                state = json.load(response)
                http = state.get('platform') == 'Android'
        except OSError:
            pass
        sample = {'seconds': round(time.monotonic() - started, 1), 'pid': pid, 'focused': focused, 'uploader': http, 'state': state}
        report['samples'].append(sample)
        print(json.dumps({**{k:sample[k] for k in ['seconds','pid','focused','uploader']}, **{k:state.get(k) for k in ['fps','inputAllowed','leftTracked','rightTracked']}}), flush=True)
        if not pid:
            break
        time.sleep(5)

    pid = capture('final-pid.txt', 'shell', 'pidof', PACKAGE)
    global_log = capture('system-startup.log', 'logcat', '-d', '-v', 'threadtime', '-T', since, 'Unity:V', 'OpenXR_SessionImpl:V', 'OpenXR_Passthrough:V', 'OpenXR_Hands:V', 'AndroidRuntime:E', 'DEBUG:E', 'ActivityManager:I', 'ActivityTaskManager:I', '*:S')
    # Unity and XR tag logs provide initialization evidence even if the process dies.
    owned_pids = {sample['pid'] for sample in report['samples'] if sample['pid']}
    owned_pids.update(re.findall(r'Start proc (\d+):' + re.escape(PACKAGE), global_log))
    relevant = '\n'.join(line for line in global_log.splitlines() if PACKAGE in line or (len(line.split()) > 2 and line.split()[2] in owned_pids))
    (folder / 'runtime.log').write_text(relevant + '\n')
    if pid:
        capture('app-pid.log', 'logcat', '-d', '-v', 'threadtime', '--pid=' + pid, '-T', since)
    capture('exit-history-after.txt', 'shell', 'dumpsys', 'activity', 'exit-info', PACKAGE)
    capture('final-activity.txt', 'shell', 'dumpsys', 'activity', 'activities')
    capture('window.txt', 'shell', 'dumpsys', 'window', 'windows')
    capture('meminfo.txt', 'shell', 'dumpsys', 'meminfo', PACKAGE)
    capture('battery.txt', 'shell', 'dumpsys', 'battery')
    try:
        screenshot = subprocess.check_output([*adb, 'exec-out', 'screencap', '-p'], timeout=15)
        if screenshot.startswith(b'\x89PNG'):
            (folder / 'headset.png').write_bytes(screenshot)
            report['checks']['screenshot_captured'] = True
    except (subprocess.CalledProcessError, subprocess.TimeoutExpired):
        report['checks']['screenshot_captured'] = False

    errors = [line for line in relevant.splitlines() if ' E Unity ' in line or 'FATAL EXCEPTION' in line or 'Fatal signal' in line or 'Exception:' in line]
    broken = [line for line in errors if "Can't add component" in line or 'Exception:' in line or 'FATAL EXCEPTION' in line or 'Fatal signal' in line]
    report['checks'].update({
        'frame_progress_seen': len({s['state'].get('frame') for s in report['samples'] if s['state']})>1,
        'process_survived': bool(pid) and all(sample['pid'] == pid for sample in report['samples']),
        'focused_at_end': bool(report['samples']) and report['samples'][-1]['focused'],
        'uploader_at_end': bool(report['samples']) and report['samples'][-1]['uploader'],
        'xr_focused_seen': 'XR_SESSION_STATE_FOCUSED' in relevant or any(s['state'].get('inputAllowed') for s in report['samples']),
        'passthrough_created_seen': 'Created Insight MR layer' in relevant,
        'hands_running_seen': any(s['state'].get('xrRunning') for s in report['samples']),
        'left_tracked_seen': any(s['state'].get('leftTracked') for s in report['samples']),
        'right_tracked_seen': any(s['state'].get('rightTracked') for s in report['samples']),
        'unity_or_crash_error_count': len(errors),
        'missing_components_or_crash_count': len(broken),
    })
    (folder / 'errors.log').write_text('\n'.join(errors) + '\n')
    (folder / 'report.json').write_text(json.dumps(report, indent=2) + '\n')
    print('Evidence:', folder)
    print(json.dumps(report['checks'], indent=2))
    print('Passthrough appearance and physical gestures still require direct headset confirmation.')
    foundation = ['frame_progress_seen', 'process_survived', 'focused_at_end', 'uploader_at_end', 'xr_focused_seen', 'passthrough_created_seen', 'hands_running_seen']
    if not all(report['checks'][key] for key in foundation) or broken:
        raise RuntimeError('ADB validation did not pass every startup check. Review ' + str(folder / 'report.json'))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--serial')
    parser.add_argument('--port', type=int, default=8790, help='Local USB-forward port; desktop studio uses 8787')
    parser.add_argument('action', choices=['build', 'install', 'connect', 'upload', 'status', 'validate'])
    parser.add_argument('image', nargs='?')
    parser.add_argument('--duration', type=int, default=30, help='Validation observation window, 10 to 120 seconds')
    parser.add_argument('--release', action='store_true', help='Build the non-development Quest release candidate')
    args = parser.parse_args()
    if args.action == 'build':
        method = 'BuildReleaseCandidate' if args.release else 'Build'
        log = 'build-release.log' if args.release else 'build.log'
        subprocess.run([UNITY, '-batchmode', '-nographics', '-quit', '-licensingIpc', 'SoftStrangeBuild6000', '-projectPath', str(ROOT), '-buildTarget', 'Android', '-executeMethod', 'SoftStrange.Editor.ProjectBuilder.' + method, '-logFile', str(ROOT / 'builds' / log)], check=True)
        if args.release:
            subprocess.run([sys.executable, str(ROOT / 'tools/sign_release.py')], check=True)
        return
    serial = device(args.serial)
    adb = [ADB, '-s', serial]
    model = run(*adb, 'shell', 'getprop', 'ro.product.model')
    if 'Quest' not in model:
        raise RuntimeError(f'Target is {model}, not a confirmed Quest. No changes made.')
    print(f'{model}: {serial}')
    if args.action == 'validate':
        validate(adb, serial, model, max(10, min(120, args.duration)), args.port)
        return
    if args.action == 'status':
        print(run(*adb, 'shell', 'pm', 'path', PACKAGE))
        return
    if args.action == 'install':
        apk = ROOT / 'builds/SoftStrangeDrawing.apk'
        print('APK SHA-256:', hashlib.sha256(apk.read_bytes()).hexdigest())
        print(run(*adb, 'install', '-r', str(apk), timeout=120))
        print(run(*adb, 'shell', 'monkey', '-p', PACKAGE, '-c', 'android.intent.category.LAUNCHER', '1'))
    print(run(*adb, 'forward', f'tcp:{args.port}', 'tcp:8787'))
    if args.action == 'upload':
        if not args.image:
            parser.error('upload requires an image path')
        data = pathlib.Path(args.image).read_bytes()
        request = urllib.request.Request(f'http://localhost:{args.port}/upload', data=data, headers={'Content-Type': 'application/octet-stream'})
        with urllib.request.urlopen(request, timeout=15) as response:
            print(response.read().decode())
    else:
        print(f'Uploader: http://localhost:{args.port} — keep the app open in the headset.')
        webbrowser.open(f'http://localhost:{args.port}')


if __name__ == '__main__':
    try:
        main()
    except (RuntimeError, OSError, subprocess.CalledProcessError, subprocess.TimeoutExpired) as error:
        print(error, file=sys.stderr)
        sys.exit(1)
