"""Author matched orthographic outlines and lightweight models from shared geometry.
Blender 4.0+: blender -b --python tools/blender/build_reference_pairs.py -- --limit 1
"""
import bpy,math,sys,json,random,argparse
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/SoftStrange/Resources/PairedReferences';OUT.mkdir(parents=True,exist_ok=True)
MODELS=ROOT/'Assets/SoftStrange/Resources/ReferenceModels';MODELS.mkdir(parents=True,exist_ok=True)
SOURCE=ROOT/'art/paired-models';SOURCE.mkdir(parents=True,exist_ok=True)
args=argparse.ArgumentParser();args.add_argument('--limit',type=int,default=60);args.add_argument('--start',type=int,default=1);opt=args.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
names=['Great white','Reef shark','Blue shark','Mako shark','Tiger shark','Whale shark','Hammerhead','Thresher','Nurse shark','Bamboo shark','Leopard shark','Blacktip shark','Little shark','Sand tiger','Lemon shark','Bull shark','Silky shark','Goblin shark','Horn shark','Sevengill shark','Sixgill shark','Copper shark','Sandbar shark','Dusky shark','Spinner shark','Oceanic whitetip','Reef swimmer','Beginner great white','Broad hammerhead','Gentle whale shark','Tiger swimmer','Longtail thresher','Blue swimmer','Baby shark','Curious shark','Gliding shark','Reef companions','Diving shark','Turning hammerhead','Spotted leopard shark','Curled bamboo shark','Epaulette shark','Wobbegong','Angel shark','Sawshark','Swift mako','Port Jackson shark','Gentle nurse shark','Blacktip swimmer','Shark companions','Sea turtle','Manta ray','Octopus','Jellyfish','Seahorse','Nautilus shell','Coral branch','Starfish and shell','Fern frond','Wildflower']
materials={}
def mat(name,color):
 if name not in materials:
  m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);materials[name]=m
 return materials[name]
def mesh(name,verts,faces,color=(.32,.57,.53)):
 m=bpy.data.meshes.new(name);m.from_pydata(verts,[],faces);m.update();o=bpy.data.objects.new(name,m);bpy.context.collection.objects.link(o);o.data.materials.append(mat('sage',color));return o

def ellipsoid(name,p,s,material='sage'):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=10,location=p);o=bpy.context.object;o.name=name;o.scale=s;o.data.materials.append(mat(material,(.05,.07,.08) if material=='eye' else (.42,.64,.59)));bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for f in o.data.polygons:f.use_smooth=True
 return o

def tube(name,points,r=.02):
 verts=[];faces=[]
 for i,p in enumerate(points):
  tangent=Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)]);tangent.normalize();u=tangent.cross(Vector((0,0,1)))
  if u.length<.01:u=tangent.cross(Vector((0,1,0)))
  u.normalize();v=tangent.cross(u).normalized()
  for j in range(8):verts.append(Vector(p)+r*(math.cos(j*math.tau/8)*u+math.sin(j*math.tau/8)*v))
 for i in range(len(points)-1):
  for j in range(8):a=i*8+j;b=i*8+(j+1)%8;faces.append((a,b,b+8,a+8))
 return mesh(name,verts,faces)

def fin(name,points,thick=.018):
 # Closed thin sculpted wedge, rather than a single double-sided sheet.
 center=sum((Vector(p) for p in points),Vector())/len(points);normal=(Vector(points[1])-Vector(points[0])).cross(Vector(points[2])-Vector(points[0])).normalized();v=list(points)+[center+normal*thick,center-normal*thick];n=len(points);faces=[]
 for i in range(n):faces.extend([(i,(i+1)%n,n),((i+1)%n,i,n+1)])
 return mesh(name,v,faces)

def shark(index):
 name=names[index-1].lower();rng=random.Random(index)
 fat=.23 if any(x in name for x in ['whale','nurse','bull','baby','little']) else .17+rng.random()*.035
 flat=any(x in name for x in ['angel','wobbegong']);fat=fat*(.6 if flat else 1)
 verts=[];faces=[]
 profile=[(-1.05,.035),(-.85,.06),(-.6,.11),(-.25,fat),(.2,fat*1.12),(.58,fat*.88),(.88,fat*.58),(1.04,.012)]
 for x,r in profile:
  for j in range(24):a=j*math.tau/24;verts.append((x,math.cos(a)*r*(1.8 if flat else 1),math.sin(a)*r))
 for i in range(len(profile)-1):
  for j in range(24):faces.append((i*24+j,i*24+(j+1)%24,(i+1)*24+(j+1)%24,(i+1)*24+j))
 faces.extend([tuple(reversed(range(24))),tuple(range((len(profile)-1)*24,len(profile)*24))]);body=mesh('Body',verts,faces)
 for p in body.data.polygons:p.use_smooth=True
 sub=body.modifiers.new('Soft sculpt','SUBSURF');sub.levels=2;sub.render_levels=2
 dorsal=.58 if 'sandbar' in name else .40
 if not flat:fin('Dorsal',[(-.38,0,fat*.8),(-.10,0,dorsal),(.18,0,fat*.8)])
 for side in [-1,1]:
  span=.72 if flat else .51
  fin('Pectoral',[(.36,side*fat*.55,-.04),(-.38,side*span,-.12),(-.18,side*fat*.75,-.07)])
  fin('Pelvic',[(-.47,side*.055,-.04),(-.74,side*.22,-.13),(-.72,side*.055,-.055)])
  ellipsoid('Eye',(.77,side*fat*.66,.055),(.029,.02,.029),'eye')
  # Raised thin gill ridges produce clear, reproducible line detail.
  for k in range(4):
   x=.30+k*.055;tube('Gill',[(x,side*fat*.94,-.04),(x+.015,side*fat*1.02,.01),(x,side*fat*.98,.065)],.004)
 tail=.98 if 'thresher' in name else .46
 fin('Tail',[(-.90,0,.02),(-1.48,0,tail),(-1.27,0,.02),(-1.43,0,-.30),(-.95,0,-.03)])
 if 'hammerhead' in name:
  ellipsoid('Hammer head',(.91,0,.01),(.19,.49,.065))
  for side in [-1,1]:ellipsoid('Hammer eye',(.96,side*.475,.02),(.026,.022,.025),'eye')
 if 'sawshark' in name or 'goblin' in name:
  fin('Rostrum',[(.86,-.055,0),(1.53,0,.015),(.86,.055,0)])
  if 'sawshark' in name:
   for k in range(8):
    for side in [-1,1]:fin('Rostral tooth',[(1+k*.058,side*.035,0),(1.025+k*.058,side*.11,0),(1.045+k*.058,side*.035,0)])
 if 'leopard' in name or 'whale' in name:
  for k in range(8):ellipsoid('Spot',(-.42+k*.15,0,fat*.93),(.025,.032,.004),'eye')
 if index in [37,50]:
  originals=list(bpy.context.scene.objects)
  for o in originals:
   if o.type!='MESH':continue
   o.location*=.70;o.location.y-=.33;o.scale*=.70;dupe=o.copy();dupe.data=o.data.copy();bpy.context.collection.objects.link(dupe);dupe.location.y+=.75;dupe.location.x+=.2

def nature(index):
 if index==51:
  ellipsoid('Shell',(0,0,0),(.65,.44,.20));ellipsoid('Head',(.77,0,.02),(.22,.14,.13))
  for side in [-1,1]:
   fin('Front flipper',[(.35,side*.25,0),(.13,side*.90,-.08),(-.12,side*.43,-.02)]);fin('Back flipper',[(-.42,side*.2,0),(-.8,side*.5,-.04),(-.67,side*.1,0)])
 elif index==52:
  ellipsoid('Body',(0,0,0),(.7,.3,.12))
  for side in [-1,1]:fin('Wing',[(.57,side*.13,0),(-.28,side*1.25,.14),(-.68,side*.15,-.02)])
  tube('Tail',[(-.58,0,0),(-1.1,0,.02),(-1.5,0,.11)],.018)
 elif index==53:
  ellipsoid('Mantle',(.1,0,.3),(.43,.38,.54))
  for j in range(8):
   a=j*math.tau/8;points=[]
   for k in range(14):t=k/13;r=.22+t*.85;points.append((math.cos(a+t*.7)*r,math.sin(a+t*.7)*r,-.10-.13*math.sin(t*math.pi)))
   tube('Arm',points,.045)
 elif index==54:
  ellipsoid('Bell',(0,0,.38),(.62,.62,.26))
  for j in range(8):a=j*math.tau/8;tube('Tentacle',[(math.cos(a)*(.28+.06*math.sin(k)),math.sin(a)*(.28+.06*math.cos(k)),.23-k*.085) for k in range(14)],.017)
 elif index==55:
  ellipsoid('Body',(0,0,.0),(.19,.15,.43));ellipsoid('Head',(.1,0,.50),(.23,.12,.16));ellipsoid('Snout',(.36,0,.48),(.20,.07,.065));tube('Curled tail',[(-.05+.28*math.cos(t),0,-.42+.28*math.sin(t)) for t in [(-math.pi/2-k*.13) for k in range(38)]],.04);fin('Back fin',[(-.16,0,.18),(-.38,0,-.04),(-.18,0,-.18)])
 elif index==56:
  points=[]
  for i in range(100):a=i*.13;r=.035+i*.007;points.append((r*math.cos(a),0,r*math.sin(a)))
  tube('Spiral shell',points,.065)
 elif index==57:
  tube('Coral trunk',[(0,0,-.7),(0,0,-.1),(.06,0,.65)],.055)
  for j in range(7):a=j*2.4;z=-.4+j*.13;tube('Branch',[(0,0,z),(.28*math.cos(a),.28*math.sin(a),z+.17),(.50*math.cos(a),.50*math.sin(a),z+.43)],.028)
 elif index==58:
  points=[]
  for j in range(10):a=j*math.pi/5;r=.85 if j%2==0 else .32;points.append((r*math.cos(a),r*math.sin(a),0))
  fin('Starfish',points,.10);ellipsoid('Small shell',(1.02,.15,0),(.23,.29,.1))
 elif index==59:
  tube('Stem',[(-.8,0,0),(0,0,.06),(.85,0,.14)],.013)
  for k in range(9):
   x=-.63+k*.155;length=.42*(1-abs(x)*.65)
   for side in [-1,1]:fin('Leaf',[(x-.09,0,.04),(x+.10,side*length,.10),(x+.16,0,.07)],.008)
 else:
  tube('Stem',[(0,0,-.85),(.05,0,-.25),(0,0,.3)],.016)
  for side in [-1,1]:fin('Leaf',[(.01,0,-.5),(.32,side*.20,-.26),(.02,0,-.24)],.01)
  for j in range(6):a=j*math.tau/6;o=ellipsoid('Petal',(.3*math.cos(a),.3*math.sin(a),.32),(.25,.15,.055));o.rotation_euler.z=a
  ellipsoid('Flower center',(0,0,.35),(.13,.13,.07))

for index in range(opt.start,min(61,opt.start+opt.limit)):
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);materials.clear()
 (shark if index<=50 else nature)(index)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0]
 bpy.ops.export_scene.fbx(filepath=str(MODELS/f'{index:02}-model.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,apply_unit_scale=True,axis_forward='-Z',axis_up='Y')
 scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE';scene.eevee.taa_render_samples=16;scene.render.resolution_x=1024;scene.render.resolution_y=1024;scene.render.resolution_percentage=100;scene.render.film_transparent=True;scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
 scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast';scene.render.use_freestyle=True;layer=scene.view_layers[0];layer.use_freestyle=True;fs=layer.freestyle_settings;fs.as_render_pass=True;ls=fs.linesets[0];ls.select_silhouette=True;ls.select_border=True;ls.select_crease=True;ls.select_external_contour=True;ls.linestyle.color=(1,1,1);ls.linestyle.alpha=1;ls.linestyle.thickness=2.8
 scene.use_nodes=True;nodes=scene.node_tree.nodes;nodes.clear();rl=nodes.new('CompositorNodeRLayers');out=nodes.new('CompositorNodeComposite');scene.node_tree.links.new(rl.outputs['Freestyle'],out.inputs['Image'])
 bpy.ops.object.camera_add();camera=bpy.context.object;camera.name='Reference camera';camera.data.type='ORTHO';scene.camera=camera
 coords=[o.matrix_world@Vector(corner) for o in meshes for corner in o.bound_box];center=Vector(tuple((min(v[a] for v in coords)+max(v[a] for v in coords))*.5 for a in range(3)))
 for view,direction in [('side',Vector((0,-1,0))),('top',Vector((0,0,1)))]:
  camera.location=center+direction*6;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler();inv=camera.rotation_euler.to_matrix().inverted();projected=[inv@(v-center) for v in coords];size=max(max(p.x for p in projected)-min(p.x for p in projected),max(p.y for p in projected)-min(p.y for p in projected));camera.data.ortho_scale=size*1.20;w=max(p.x for p in projected)-min(p.x for p in projected);h=max(p.y for p in projected)-min(p.y for p in projected);scene.render.resolution_x=max(256,int(1024*w/size));scene.render.resolution_y=max(256,int(1024*h/size))
  scene.render.filepath=str(OUT/f'{index:02}-{view}.png');bpy.ops.render.render(write_still=True)
 if index==1:bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'reference-authoring.blend'))
 (SOURCE/f'{index:02}.json').write_text(json.dumps({'id':f'{index:02}','name':names[index-1],'authoring':'original procedural stylized mesh','script':'tools/blender/build_reference_pairs.py','views':['side','top'],'sourceModel':f'ReferenceModels/{index:02}-model','scientificAccuracy':'stylized drawing study; not an anatomical reference'},indent=2))
 print('PAIR COMPLETE',index,names[index-1],flush=True)
# Only publish entries that have both views and a mesh.
rows=[]
for i,name in enumerate(names,1):
 if all((OUT/f'{i:02}-{v}.png').exists() for v in ['side','top']):rows.append(f'{i:02}|{name}|PairedReferences/{i:02}-side|PairedReferences/{i:02}-top|ReferenceModels/{i:02}-model')
(ROOT/'Assets/SoftStrange/Resources/PairedCatalog.txt').write_text('\n'.join(rows)+'\n')
