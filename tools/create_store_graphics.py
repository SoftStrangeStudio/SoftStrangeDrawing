#!/usr/bin/env python3
"""Author editable vector brand graphics and export the current Meta cover sizes.
These are branded illustrations, never represented as gameplay screenshots.
"""
from pathlib import Path
import json
import pypdfium2 as pdfium
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'art/store';OUT.mkdir(exist_ok=True)
pdfmetrics.registerFont(TTFont('Studio',str(ROOT/'Assets/TextMesh Pro/Fonts/LiberationSans.ttf')))
FOREST='#17392E';SAGE='#315246';CREAM='#F7EDCF';GOLD='#EAB864'
def shark(c,x,y,size):
 c.saveState();c.translate(x,y);c.scale(size/300,size/300);c.setStrokeColor(CREAM);c.setLineWidth(5);c.setLineCap(1);c.setLineJoin(1)
 p=c.beginPath();p.moveTo(-105,-40);p.curveTo(-15,-115,115,-60,120,10);p.curveTo(115,45,75,62,38,65);p.curveTo(5,108,-28,112,-40,98);p.curveTo(-18,65,-15,60,-28,45);p.curveTo(-90,43,-116,0,-105,-40);c.drawPath(p)
 p=c.beginPath();p.moveTo(-105,-40);p.lineTo(-145,-20);p.lineTo(-130,-75);p.lineTo(-92,-60);c.drawPath(p)
 p=c.beginPath();p.moveTo(-5,5);p.curveTo(6,-23,25,-28,52,-15);c.drawPath(p)
 p=c.beginPath();p.moveTo(73,23);p.curveTo(85,12,103,11,112,19);c.drawPath(p)
 c.setFillColor(CREAM);c.circle(69,38,4,fill=1,stroke=0)
 for xx in [-28,-15,-2]:
  p=c.beginPath();p.moveTo(xx,32);p.curveTo(xx-6,23,xx-6,17,xx,10);c.drawPath(p)
 c.saveState();c.translate(20,-28);c.rotate(-35);c.setFillColor(GOLD);c.roundRect(-7,-22,14,70,5,fill=1,stroke=0);p=c.beginPath();p.moveTo(-7,-26);p.lineTo(7,-26);p.lineTo(0,-42);p.close();c.drawPath(p,fill=1,stroke=0);c.restoreState()
 c.setStrokeColor(GOLD);c.setLineWidth(3);c.line(114,80,114,102);c.line(103,91,125,91);c.restoreState()
def title(c,x,y,size):
 c.setFillColor(CREAM);c.setFont('Studio',size);c.drawCentredString(x,y+size*.15,'Soft Strange');c.drawCentredString(x,y-size*1.0,'Drawing')
specs={'hero-cover':(3000,900),'cover-landscape':(2560,1440),'cover-square':(1440,1440),'cover-portrait':(1008,1440),'mini-landscape':(1080,360),'logo-transparent':(1800,650),'icon-vector':(512,512)}
report=[]
for name,(w,h) in specs.items():
 pdf=OUT/(name+'.pdf');c=canvas.Canvas(str(pdf),pagesize=(w,h));c.setTitle('Soft Strange Drawing — '+name);transparent=name=='logo-transparent'
 if not transparent:
  c.setFillColor(FOREST);c.rect(0,0,w,h,fill=1,stroke=0)
  if name!='icon-vector':
   c.setStrokeColor(SAGE);c.setLineWidth(max(2,w*.003));c.circle(w*.04,h*.02,h*.36,fill=0,stroke=1);c.circle(w*.96,h*.97,h*.3,fill=0,stroke=1)
 if name=='icon-vector':shark(c,w*.52,h*.49,w*.82)
 elif transparent:shark(c,w*.20,h*.51,h*.76);title(c,w*.62,h*.52,h*.25)
 elif w/h>2.4:shark(c,w*.25,h*.49,h*.68);title(c,w*.61,h*.54,h*.17)
 elif w/h>1.3:shark(c,w*.27,h*.51,h*.68);title(c,w*.65,h*.53,h*.14)
 else:shark(c,w*.52,h*.65,w*.70);title(c,w*.5,h*.39,min(w*.118,h*.105))
 c.showPage();c.save();doc=pdfium.PdfDocument(str(pdf));bitmap=doc[0].render(fill_color=(0,0,0,0) if transparent else (255,255,255,255));pix=bitmap.to_pil();pix.save(OUT/(name+'.png'));report.append({'asset':name,'width':pix.width,'height':pix.height,'channels':len(pix.getbands()),'transparent':transparent,'source':str(pdf.relative_to(ROOT))})
(OUT/'manifest.json').write_text(json.dumps({'status':'Brand artwork drafts; review with final listing. Not gameplay footage.','source':'https://developers.meta.com/vr/resources/asset-guidelines/','font':'Liberation Sans, SIL Open Font License','assets':report},indent=2))
print(json.dumps(report,indent=2))
