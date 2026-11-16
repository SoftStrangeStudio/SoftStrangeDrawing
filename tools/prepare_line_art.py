#!/usr/bin/env python3
"""Remove paper/white fill and preserve dark ink as recolorable white RGBA strokes."""
import argparse
import pathlib
from PIL import Image, ImageFilter
import numpy as np


def convert(source, destination, cutoff=170, feather=55, clean=False):
    rgba = np.array(Image.open(source).convert('RGBA'), dtype=np.float32)
    luminance = rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722])
    ink = np.clip((cutoff - luminance) / max(feather, 1), 0, 1)
    alpha = np.rint(ink * rgba[:, :, 3]).astype(np.uint8)
    if clean:
        # Discard generated haze, then strengthen antialiased ink without filling holes.
        alpha = np.rint(np.clip((alpha.astype(np.float32) - 20) / 190, 0, 1) * 255).astype(np.uint8)
        # Eight-connected components: keep eyes and short details, remove isolated dust.
        mask = alpha > 0
        visited = np.zeros(mask.shape, dtype=bool)
        height, width = mask.shape
        for y, x in zip(*np.nonzero(mask)):
            if visited[y, x]:
                continue
            stack = [(y, x)]; visited[y, x] = True; component = []
            while stack:
                yy, xx = stack.pop(); component.append((yy, xx))
                for dy, dx in ((-1,-1),(-1,0),(-1,1),(0,-1),(0,1),(1,-1),(1,0),(1,1)):
                    ny, nx = yy + dy, xx + dx
                    if 0 <= ny < height and 0 <= nx < width and mask[ny,nx] and not visited[ny,nx]:
                        visited[ny,nx] = True; stack.append((ny,nx))
            if len(component) < 5:
                yy, xx = zip(*component); alpha[yy,xx] = 0
        # Convert broad dark fill (fin tips, pupils) to a boundary, retaining transparency.
        core = np.asarray(Image.fromarray(alpha).filter(ImageFilter.MinFilter(11)))
        alpha = np.minimum(alpha, 255 - core)
        # A small expansion stabilizes delicate AI strokes at the headset's viewing scale.
        alpha = np.asarray(Image.fromarray(alpha).filter(ImageFilter.MaxFilter(3)))
    result = np.full(rgba.shape, 255, dtype=np.uint8)
    result[:, :, 3] = alpha
    if clean:
        ys, xs = np.nonzero(alpha)
        if not len(xs):
            raise ValueError(f'No ink survived cleanup: {source}')
        cropped = Image.fromarray(result[ys.min():ys.max()+1,xs.min():xs.max()+1], 'RGBA')
        cropped.thumbnail((1472,1472), Image.Resampling.LANCZOS)
        padded = Image.new('RGBA', (cropped.width+64,cropped.height+64), (255,255,255,0))
        padded.paste(cropped, (32,32))
        result = np.array(padded)
        result[:,:,:3] = 255
        result[:,:,3][result[:,:,3] < 4] = 0
        alpha = result[:,:,3]
    destination.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(result, 'RGBA').save(destination)
    return {'width': result.shape[1], 'height': result.shape[0], 'stroke_pixels': int((alpha > 0).sum()), 'transparent_pixels': int((alpha == 0).sum())}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=pathlib.Path)
    parser.add_argument('destination', type=pathlib.Path)
    parser.add_argument('--cutoff', type=float, default=170)
    parser.add_argument('--feather', type=float, default=55)
    parser.add_argument('--clean', action='store_true', help='Remove haze/dust, hollow broad dark fills and strengthen thin strokes.')
    args = parser.parse_args()
    sources = sorted(args.source.glob('*.png')) if args.source.is_dir() else [args.source]
    for source in sources:
        destination = args.destination / source.name if args.source.is_dir() else args.destination
        if source.resolve() == destination.resolve():
            parser.error('Choose a separate output path; originals are preserved.')
        print(source.name, convert(source, destination, args.cutoff, args.feather, args.clean))


if __name__ == '__main__':
    main()
