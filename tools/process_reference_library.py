#!/usr/bin/env python3
"""Rebuild the preserved AI library into white, transparent drawing references."""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np
from prepare_line_art import convert

ROOT = Path(__file__).resolve().parent.parent

def main():
    resources = ROOT / 'Assets/SoftStrange/Resources'
    report = {'pipeline': 'AI originals -> luminance/alpha extraction -> haze and dust cleanup -> fill boundary -> stroke strengthening -> padded RGBA', 'images': []}
    preview = Image.new('RGB', (1200, 1400), (24,43,35))
    pen = ImageDraw.Draw(preview)
    for index, row in enumerate((resources / 'ReferenceCatalog.txt').read_text().splitlines()):
        name, title = row.split('|')
        source = ROOT / 'art/source' / (name+'.png')
        destination = resources / 'References' / (name+'.png')
        stats = convert(source,destination,cutoff=150,feather=65,clean=True)
        with Image.open(destination) as image:
            rgba = np.array(image)
            alpha = rgba[:,:,3]
            checks = {'white_rgb': bool((rgba[:,:,:3]==255).all()), 'clear_border': bool(not alpha[:24].any() and not alpha[-24:].any() and not alpha[:,:24].any() and not alpha[:,-24:].any()), 'mostly_transparent': float((alpha==0).mean()) > .65, 'visible_ink': int((alpha>128).sum())>100}
            if not all(checks.values()):
                raise ValueError(f'Image failed asset validation: {name}: {checks}')
            image.thumbnail((190,112),Image.Resampling.LANCZOS)
            x=(index%6)*200+(200-image.width)//2;y=(index//6)*140
            preview.paste(image,(x,y),image);pen.text(((index%6)*200+8,y+116),name,fill=(234,230,210))
        record = {'id':name,'title':title,'source':str(source.relative_to(ROOT)),'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'output':str(destination.relative_to(ROOT)),'output_sha256':hashlib.sha256(destination.read_bytes()).hexdigest(),'filter':{'cutoff':150,'feather':65,'clean':True},'validation':stats,'checks':checks}
        report['images'].append(record)
        print(name,stats,flush=True)
    (ROOT/'art/provenance/filtered-library.json').write_text(json.dumps(report,indent=2)+'\n')
    preview.save(ROOT/'builds/ai-filtered-library.jpg',quality=95)
    print('Validated',len(report['images']),'AI illustrations')

if __name__ == '__main__':
    main()
