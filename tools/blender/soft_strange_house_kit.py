"""Soft Strange House Kit: local Blender add-on and reproducible scene authoring."""
bl_info={'name':'Soft Strange House Kit','author':'Soft Strange','version':(1,0,0),'blender':(4,0,0),'category':'Object'}
import bpy,math,random,pathlib
from mathutils import Vector
random.seed(42)

def material(name,color,roughness=.6,metallic=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
 p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Roughness'].default_value=roughness;p.inputs['Metallic'].default_value=metallic
 return m

def cube(name,loc,size,mat,bevel=.02):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.name=name;o.dimensions=size;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
 if bevel:
  mod=o.modifiers.new('Soft manufactured edges','BEVEL');mod.width=min(bevel,min(size)/3);mod.segments=3
  o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL')
 return o

def cylinder(name,loc,radius,depth,mat,vertices=32):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc);o=bpy.context.object;o.name=name;o.data.materials.append(mat)
 bevel=o.modifiers.new('Rounded rim','BEVEL');bevel.width=.008;bevel.segments=3;o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');return o

def sphere(name,loc,size,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=loc);o=bpy.context.object;o.name=name;o.scale=size;o.data.materials.append(mat)
 for p in o.data.polygons:p.use_smooth=True
 return o

def torus(name,loc,major,minor,mat,rotation=(0,0,0)):
 bpy.ops.mesh.primitive_torus_add(major_radius=major,minor_radius=minor,major_segments=32,minor_segments=12,location=loc,rotation=rotation);o=bpy.context.object;o.name=name;o.data.materials.append(mat);return o

def anchor(name,loc):
 o=bpy.data.objects.new(name,None);bpy.context.collection.objects.link(o);o.location=loc;return o

def build():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 oak=material('Honey oak',(.45,.24,.10),.48);darkoak=material('Oak end grain',(.28,.13,.055),.65);cream=material('Warm plaster',(.84,.79,.69),.9);trim=material('Ivory painted wood',(.94,.91,.84),.45)
 sage=material('Sage cabinetry',(.23,.34,.28),.48);linen=material('Oatmeal linen',(.65,.57,.44),.9);rug=material('Terracotta woven rug',(.45,.18,.10),.95);ink=material('Charcoal',(.025,.035,.04),.45)
 brass=material('Brushed brass',(.55,.36,.12),.32,.8);steel=material('Brushed steel',(.45,.48,.50),.27,.85);paper=material('Drawing paper',(.95,.93,.87),.9);green=material('Leaf green',(.08,.23,.10),.8);clay=material('Clay ceramic',(.52,.24,.13),.72);blue=material('Muted blue',(.14,.24,.31),.85)
 glass=material('Window glass',(.48,.65,.72),.15);p=glass.node_tree.nodes.get('Principled BSDF');p.inputs['Alpha'].default_value=.18;glass.diffuse_color=(.48,.65,.72,.18)
 # Floors are individually authored planks with gentle tonal variation.
 for y in range(24):
  for x in range(6):
   m=material(f'Oak plank {y:02}-{x}',tuple(max(0,min(1,c+random.uniform(-.035,.035))) for c in (.43,.25,.12)),.55)
   cube(f'Floor plank {y:02}-{x}',(-2.5+x,-2.875+y*.25,-.045),(1-.008,.25-.005,.09),m,.005)
 cube('Back wall',(0,3,1.5),(6,.14,3),cream)
 cube('Left wall',(-3,0,1.5),(.14,6,3),cream)
 # Front wall doorway and right wall window bays.
 cube('Front wall left',(-2,-3,1.5),(2,.14,3),cream);cube('Front wall right',(1.65,-3,1.5),(2.7,.14,3),cream);cube('Door header',(-.65,-3,2.7),(1.3,.14,.6),cream)
 cube('Right wall sill',(3,0,.48),(.14,6,.96),cream);cube('Right wall lintel',(3,0,2.78),(.14,6,.44),cream)
 for y in (-2.7,0,2.7):cube('Window pier',(3,y,1.72),(.14,.32,1.55),cream)
 for y in (-1.35,1.35):
  cube('Window pane',(3.02,y,1.72),(.025,2.3,1.45),glass,.003)
  for z in (1,2.45):cube('Window frame horizontal',(2.91,y,z),(.10,2.46,.055),trim,.004)
  for edge in (-1.2,0,1.2):cube('Window mullion',(2.91,y+edge,1.72),(.10,.055,1.5),trim,.004)
 for y in (-2.90,2.90):cube('Baseboard long',(0,y,.085),(5.8,.06,.17),trim,.006)
 for x in (-2.90,2.90):cube('Baseboard side',(x,0,.085),(.06,5.8,.17),trim,.006)
 door=cube('Oak entrance door',(-.67,-3.01,1.05),(1.10,.06,2.1),oak,.008);door.rotation_euler.z=math.radians(-22)
 cylinder('Door brass knob',(-.22,-2.88,1.02),.028,.075,brass)
 # Main drawing desk and a comfortable chair.
 cube('Drawing desk top',(.2,.55,.78),(1.7,.9,.075),oak,.025)
 for x in (-.52,.92):
  for y in (.23,.88):cube('Desk tapered leg',(x,y,.365),(.055,.055,.73),darkoak,.008)
 cube('Desk apron',(.2,.9,.69),(1.5,.05,.14),darkoak,.008)
 cube('Drawing sheet',(.05,.40,.823),(.43,.31,.002),paper,.0005)
 cube('Pencil',(.54,.38,.829),(.008,.20,.008),ink,.002).rotation_euler.z=-.18
 cube('Eraser',(.53,.60,.836),(.045,.025,.02),trim,.006)
 cylinder('Ceramic mug',(-.43,.72,.875),.052,.11,clay);torus('Mug handle',(-.5,.72,.875),.035,.008,clay,(math.pi/2,0,0));cylinder('Coffee surface',(-.43,.72,.930),.044,.002,darkoak)
 cylinder('Desk lamp base',(.82,.84,.836),.07,.025,brass)
 arm=cylinder('Lamp upright',(.82,.84,1.02),.012,.35,brass);sphere('Lamp shade',(.75,.8,1.18),(.10,.10,.065),sage)
 cube('Chair seat',(.15,-.37,.46),(.46,.44,.065),oak,.03)
 for x in (-.04,.34):
  for y in (-.54,-.2):cube('Chair leg',(x,y,.23),(.035,.035,.46),darkoak,.007)
 cube('Chair curved back',(.15,-.59,.83),(.47,.045,.43),oak,.025)
 for x in (-.035,.335):cube('Chair back support',(x,-.57,.65),(.026,.03,.40),darkoak,.006)
 # Living space with constructed cushions and reading accessories.
 cube('Living room rug',(-1.6,-1.25,.018),(2.0,2.2,.018),rug,.009)
 cube('Sofa base',(-2.30,-.5,.28),(.9,1.8,.28),linen,.09);cube('Sofa back',(-2.68,-.5,.67),(.18,1.85,.65),linen,.08)
 for y in (-1.35,.35):cube('Sofa arm',(-2.30,y,.58),(.9,.18,.53),linen,.07)
 for y in (-.94,-.08):cube('Sofa seat cushion',(-2.23,y,.47),(.68,.74,.15),linen,.065)
 sphere('Blue throw cushion',(-2.43,-.85,.75),(.15,.29,.23),blue);sphere('Cream throw cushion',(-2.43,.02,.75),(.15,.27,.24),trim)
 cube('Coffee table top',(-1.12,-.60,.40),(.75,1.12,.055),oak,.055)
 for x in (-1.38,-.86):
  for y in (-1.01,-.19):cube('Coffee table leg',(x,y,.2),(.04,.04,.4),darkoak,.008)
 for i in range(3):cube('Stacked art book',(-1.17,-.55,.445+i*.022),(.28,.36,.022),(sage,blue,trim)[i],.003)
 # Compact kitchen: cabinets, oven, sink and open shelving.
 for x in (-1.6,-.7,.2,1.1):
  cube('Kitchen cabinet',(x,2.57,.46),(.85,.64,.88),sage,.018);cube('Cabinet face',(x,2.225,.48),(.80,.04,.78),sage,.008)
  handle=cylinder('Cabinet pull',(x,2.19,.67),.006,.19,brass);handle.rotation_euler.y=math.pi/2
 cube('Stone countertop',(-.25,2.55,.94),(3.65,.72,.075),trim,.016)
 cube('Oven front',(.20,2.19,.44),(.66,.045,.67),steel,.015);cube('Oven glass',(.20,2.16,.44),(.52,.015,.38),ink,.015)
 for x in (-.0,.39):
  for y in (2.38,2.70):torus('Stove burner',(x,y,.984),.07,.008,ink)
 cube('Sink bowl',(-.85,2.48,.962),(.57,.42,.02),steel,.06)
 cylinder('Faucet stem',(-.85,2.75,1.09),.017,.23,steel);cube('Faucet spout',(-.85,2.68,1.19),(.035,.18,.035),steel,.016)
 cube('Refrigerator',(2.14,2.46,.97),(.76,.77,1.94),trim,.035);cube('Fridge door',(2.14,2.06,.99),(.70,.035,1.86),trim,.025);cube('Fridge handle',(2.43,2.02,1.17),(.025,.03,.42),steel,.01)
 for z in (1.53,2.02):
  cube('Open kitchen shelf',(-.62,2.76,z),(2.50,.36,.045),oak,.012)
  for i in range(5):cylinder('Shelf ceramic bowl',(-1.6+i*.43,2.70,z+.08),.065,.11,(clay,trim,sage)[i%3])
 # Bookcase and individually authored books, potted plants and framed artwork.
 cube('Bookcase left',(-2.67,1.64,1.1),(.07,.7,2.2),oak,.01);cube('Bookcase right',(-1.68,1.64,1.1),(.07,.7,2.2),oak,.01)
 for z in (.1,.65,1.2,1.75,2.2):cube('Bookcase shelf',(-2.18,1.64,z),(1.07,.7,.045),oak,.008)
 for row in range(3):
  for i in range(8):cube('Art library book',(-2.6+i*.12,1.4,.35+row*.55),(.07,.35,.36+random.uniform(-.04,.04)),(sage,blue,clay,trim)[(i+row)%4],.003)
 for px,py in ((2.40,-2.2),(-2.2,1.65)):
  pz=.18 if px>0 else 2.30;cylinder('Plant pot',(px,py,pz),.16,.28,clay)
  for i in range(12):
   ang=i*2.4;leaf=sphere('Plant leaf',(px+.17*math.cos(ang),py+.17*math.sin(ang),pz+.25+i*.025),(.10,.025,.19),green);leaf.rotation_euler=(.45*math.sin(ang),.5*math.cos(ang),ang)
 cube('Wall art frame',(0,2.90,1.70),(.95,.045,.65),darkoak,.012);cube('Wall art paper',(0,2.866,1.70),(.85,.005,.55),paper,.002)
 for i in range(7):cube('Abstract art stripe',(-.3+i*.1,2.858,1.7),(.045,.005,.18+abs(math.sin(i))*.20),(sage,blue,clay)[i%3],.002)
 # Named anchors are portable across Unity, Rust or other importers.
 anchor('UserEye',(.12,-1.20,1.48));anchor('DrawingDeskAnchor',(.05,.40,.825));anchor('ReferenceAnchor',(.05,.40,1.04));anchor('HouseOrigin',(0,0,0))
 bpy.ops.object.light_add(type='AREA',location=(1,-1,3.5));light=bpy.context.object;light.name='Soft daylight';light.data.energy=350;light.data.shape='DISK';light.data.size=4
 bpy.ops.object.light_add(type='AREA',location=(-1,2,2.8));bpy.context.object.data.energy=110;bpy.context.object.data.size=2
 bpy.context.scene.world.color=(.18,.20,.23)
 bpy.ops.object.camera_add(location=(2.65,-2.65,2.10));camera=bpy.context.object;camera.name='House presentation camera';camera.rotation_euler=(Vector((-.4,1.1,1.05))-camera.location).to_track_quat('-Z','Y').to_euler();bpy.context.scene.camera=camera;camera.data.lens=22
 bpy.context.scene.render.engine='CYCLES';bpy.context.scene.cycles.samples=48;bpy.context.scene.render.resolution_x=1600;bpy.context.scene.render.resolution_y=1100;bpy.context.scene.render.resolution_percentage=100
 return bpy.context.scene

class SOFTSTRANGE_OT_build_house(bpy.types.Operator):
 bl_idname='softstrange.build_house';bl_label='Build Soft Strange house'
 def execute(self,context):build();return {'FINISHED'}

def register():bpy.utils.register_class(SOFTSTRANGE_OT_build_house)
def unregister():bpy.utils.unregister_class(SOFTSTRANGE_OT_build_house)

def export(root):
 scene=build();directory=pathlib.Path(root)/'Assets/SoftStrange/Environment';directory.mkdir(parents=True,exist_ok=True)
 source=pathlib.Path(root)/'art/environment';source.mkdir(parents=True,exist_ok=True)
 bpy.ops.wm.save_as_mainfile(filepath=str(source/'DrawingHouse.blend'))
 bpy.ops.export_scene.fbx(filepath=str(directory/'DrawingHouse.fbx'),use_selection=False,object_types={'MESH','EMPTY'},apply_unit_scale=True,bake_space_transform=True,axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,path_mode='COPY',embed_textures=True)
 preview=pathlib.Path(root)/'builds/house-preview.png';scene.render.filepath=str(preview);bpy.ops.render.render(write_still=True)
 objects=[{'name':o.name,'type':o.type,'vertices':len(o.data.vertices) if o.type=='MESH' else 0} for o in scene.objects]
 import json
 (pathlib.Path(root)/'docs/house-assets.json').write_text(json.dumps(objects,indent=2))

if __name__=='__main__':
 import sys
 root=sys.argv[sys.argv.index('--')+1] if '--' in sys.argv else str(pathlib.Path(__file__).resolve().parents[2])
 export(root)
