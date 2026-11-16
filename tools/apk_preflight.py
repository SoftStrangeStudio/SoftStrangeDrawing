#!/usr/bin/env python3
"""Inspect existing APKs without installing or changing signing identity."""
import argparse, hashlib, json, os, pathlib, re, subprocess, zipfile
ROOT = pathlib.Path(__file__).resolve().parent.parent
ANDROID = pathlib.Path('/Applications/Unity/Hub/Editor/6000.3.11f1/PlaybackEngines/AndroidPlayer')
BUILD_TOOLS = ANDROID / 'SDK/build-tools/36.0.0'

def inspect(apk):
    steam_frame = apk.name.endswith('-SteamFrame.apk')
    env = dict(os.environ, JAVA_HOME=str(ANDROID / 'OpenJDK'))
    def run(*args):
        return subprocess.check_output(args, text=True, env=env, stderr=subprocess.STDOUT)
    badging = run(str(BUILD_TOOLS / 'aapt'), 'dump', 'badging', str(apk))
    manifest = run(str(BUILD_TOOLS / 'aapt'), 'dump', 'xmltree', str(apk), 'AndroidManifest.xml')
    signing = run(str(BUILD_TOOLS / 'apksigner'), 'verify', '--verbose', '--print-certs', str(apk))
    # Verification normally selects v3 for minSdk 32. Verify v2 separately
    # against its Android API range rather than misreporting it as absent.
    signing_v2 = run(str(BUILD_TOOLS / 'apksigner'), 'verify', '--verbose', '--min-sdk-version', '24', '--max-sdk-version', '27', str(apk))
    permissions = re.findall(r"uses-permission: name='([^']+)'", badging)
    with zipfile.ZipFile(apk) as archive:
        abis = sorted({p.split('/')[1] for p in archive.namelist() if p.startswith('lib/') and p.endswith('.so')})
    checks = {
        'under_1GB': apk.stat().st_size < 1_000_000_000,
        'arm64_only': abis == ['arm64-v8a'],
        'signature_v2': 'Verified using v2 scheme (APK Signature Scheme v2): true' in signing_v2,
        'vr_category': 'com.oculus.intent.category.VR' in manifest,
        'focus_aware': 'com.oculus.vr.focusaware' in manifest,
        'contextual_passthrough': 'passthrough-contextual' in manifest,
        'no_raw_camera_or_microphone_permission': not any(p in permissions for p in ['android.permission.CAMERA','horizonos.permission.HEADSET_CAMERA','com.oculus.permission.USE_PASSTHROUGH_CAMERA','android.permission.RECORD_AUDIO']),
        'non_debuggable': not re.search(r'android:debuggable[^\n]*0xffffffff', manifest),
        'not_android_debug_key': 'Android Debug' not in signing,
        'no_internet_permission': 'android.permission.INTERNET' not in permissions,
    }
    if steam_frame:
        for name in ['vr_category','focus_aware','contextual_passthrough']:
            checks.pop(name)
        minimum = re.search(r"sdkVersion:'(\d+)'",badging)
        checks.update({
            'lepton_min_api_at_most_30': minimum is not None and int(minimum.group(1)) <= 30,
            'launcher_activity': 'launchable-activity:' in badging,
            'openxr_permission': 'org.khronos.openxr.permission.OPENXR' in permissions,
            'no_meta_manifest_dependencies': 'com.oculus.' not in manifest and 'oculus.software.' not in manifest,
        })
    else:
        checks['quest_target_sdk_34'] = "targetSdkVersion:'34'" in badging
        checks['quest_install_location_auto'] = bool(re.search(r'android:installLocation[^\n]*0x0\b', manifest))
        checks['quest_excluded_from_recents'] = bool(re.search(r'android:excludeFromRecents[^\n]*0xffffffff', manifest))
    stem = apk.stem
    folder = ROOT / 'builds/package-checks'; folder.mkdir(exist_ok=True)
    for name, content in [('badging',badging),('manifest',manifest),('signature',signing),('signature-v2',signing_v2)]:
        (folder / f'{stem}-{name}.txt').write_text(content)
    limits = 'Package inspection only. Physical Steam Frame acceptance and Steam distribution remain separate gates.' if steam_frame else 'Package inspection only. Publisher information, hardware acceptance and Meta review remain separate gates.'
    if not checks['not_android_debug_key']:
        limits += ' Android test-key signing must be replaced before submission.'
    report = {'target':'Steam Frame' if steam_frame else 'Quest','apk':str(apk),'bytes':apk.stat().st_size,'sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'abis':abis,'permissions':permissions,'checks':checks,'store_approved':False,'limits':limits}
    (folder / f'{stem}.json').write_text(json.dumps(report,indent=2)+'\n')
    return report

if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('apk',nargs='+',type=pathlib.Path)
    args=parser.parse_args()
    for apk in args.apk:print(json.dumps(inspect(apk.resolve()),indent=2))
