#!/usr/bin/env python3
import importlib.util
import json
import pathlib
import shutil
import sys

root = pathlib.Path(__file__).resolve().parent.parent
spec = importlib.util.spec_from_file_location('lineart', root / 'tools/prepare_line_art.py')
lineart = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lineart)
entry = json.loads(sys.argv[1])
name = f"{entry['id']:02d}-study"
source = pathlib.Path(entry['source'])
original = root / 'art/source' / (name + '.png')
shutil.copy2(source, original)
destination = root / 'Assets/SoftStrange/Resources/References' / (name + '.png')
entry['filter'] = {'cutoff': 150, 'feather': 65, 'clean': True}
entry['validation'] = lineart.convert(original, destination, **entry['filter'])
entry['asset'] = str(destination.relative_to(root))
(root / 'art/provenance' / (name + '.json')).write_text(json.dumps(entry, indent=2))
print(name, entry['validation'])
