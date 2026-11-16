#!/usr/bin/env python3
"""Deploy the Steam Frame APK to a user-specified Lepton Development endpoint."""
import argparse
import hashlib
import subprocess
from headset import ADB, ROOT, PACKAGE

def run(*args):
    return subprocess.check_output([ADB,*args],text=True,timeout=30).strip()

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('target',help='Frame hostname/IP: start Lepton Development first; use localhost:5555 after USB port forwarding.')
    parser.add_argument('--logs',action='store_true',help='Read bounded Unity/crash logs after launching')
    args=parser.parse_args()
    endpoint=args.target if ':' in args.target else args.target+':5555'
    print(run('connect',endpoint))
    model=run('-s',endpoint,'shell','getprop','ro.product.model')
    manufacturer=run('-s',endpoint,'shell','getprop','ro.product.manufacturer')
    abi=run('-s',endpoint,'shell','getprop','ro.product.cpu.abi')
    if 'quest' in model.lower() or 'oculus' in manufacturer.lower() or abi!='arm64-v8a':
        raise RuntimeError(f'Expected the specified ARM64 Lepton endpoint, got {model}, {manufacturer}, {abi}. Installation stopped.')
    apk=ROOT/'builds/SoftStrangeDrawing-SteamFrame.apk'
    print('Target:',endpoint,'model:',model,'APK SHA256:',hashlib.sha256(apk.read_bytes()).hexdigest())
    subprocess.run([ADB,'-s',endpoint,'install','-r',str(apk)],check=True,timeout=120)
    print(run('-s',endpoint,'shell','am','start','-W','-n',PACKAGE+'/com.unity3d.player.UnityPlayerGameActivity'))
    print(run('-s',endpoint,'forward','tcp:8791','tcp:8787'))
    if args.logs:
        print(run('-s',endpoint,'logcat','-d','-t','300','Unity:I','AndroidRuntime:E','*:S'))

if __name__=='__main__':
    main()
