#!/usr/bin/env python3
"""Create a protected local release identity and sign a non-development APK.

Back up the entire signing directory securely. Never commit its contents.
"""
import argparse
import json
import os
from pathlib import Path
import secrets
import subprocess

ROOT = Path(__file__).resolve().parent.parent
ANDROID = Path('/Applications/Unity/Hub/Editor/6000.3.11f1/PlaybackEngines/AndroidPlayer')
TOOLS = ANDROID / 'SDK/build-tools/36.0.0'
SIGNING = Path.home() / '.local/share/SoftStrangeDrawing/signing'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--create', action='store_true', help='Create identity if none exists; never replace a key')
    parser.add_argument('--input', type=Path, default=ROOT / 'builds/SoftStrangeDrawing-ReleaseCandidate.apk')
    parser.add_argument('--output', type=Path, default=ROOT / 'builds/SoftStrangeDrawing-QuestRelease.apk')
    args = parser.parse_args()
    os.umask(0o077)
    config = SIGNING / 'identity.json'
    keystore = SIGNING / 'softstrange-release.p12'
    env = dict(os.environ, JAVA_HOME=str(ANDROID / 'OpenJDK'))
    if not config.exists():
        if not args.create:
            parser.error('No signing identity. Use --create only when creating a new release identity is intended.')
        if keystore.exists():
            parser.error('Keystore already exists without identity metadata; refusing to replace it.')
        SIGNING.mkdir(parents=True, exist_ok=True, mode=0o700)
        SIGNING.chmod(0o700)
        identity = {'alias': 'softstrange-release', 'password': secrets.token_urlsafe(48)}
        # Write the password first so an interrupted keytool run cannot orphan a key.
        with config.open('x') as file:
            json.dump(identity, file)
        env['SSD_SIGNING_PASSWORD'] = identity['password']
        subprocess.run([str(ANDROID / 'OpenJDK/bin/keytool'), '-genkeypair', '-keystore', str(keystore),
                        '-storetype', 'PKCS12', '-alias', identity['alias'], '-keyalg', 'RSA', '-keysize', '3072',
                        '-validity', '10000', '-dname', 'CN=Soft Strange Drawing',
                        '-storepass:env', 'SSD_SIGNING_PASSWORD'], env=env, check=True)
    identity = json.loads(config.read_text())
    if not keystore.is_file():
        parser.error('Identity metadata exists but its keystore is missing; restore the key before signing.')
    if args.input.resolve() == args.output.resolve():
        parser.error('Input and output must differ to preserve the unsigned/test-signed candidate.')
    env['SSD_SIGNING_PASSWORD'] = identity['password']
    badging = subprocess.check_output([str(TOOLS / 'aapt'), 'dump', 'badging', str(args.input)], text=True)
    if 'application-debuggable' in badging:
        parser.error('Refusing to release-sign a debuggable APK.')
    args.output.parent.mkdir(parents=True, exist_ok=True)
    subprocess.run([str(TOOLS / 'apksigner'), 'sign', '--ks', str(keystore), '--ks-key-alias', identity['alias'],
                    '--ks-pass', 'env:SSD_SIGNING_PASSWORD', '--key-pass', 'env:SSD_SIGNING_PASSWORD',
                    '--v2-signing-enabled', 'true', '--v3-signing-enabled', 'true',
                    '--out', str(args.output), str(args.input)], env=env, check=True)
    subprocess.run([str(TOOLS / 'apksigner'), 'verify', '--verbose', '--print-certs', str(args.output)], env=env, check=True)
    subprocess.run([str(TOOLS / 'zipalign'), '-c', '-P', '16', '4', str(args.output)], check=True)
    print('Signed APK:', args.output)
    print('Back up this private directory securely:', SIGNING)


if __name__ == '__main__':
    main()
