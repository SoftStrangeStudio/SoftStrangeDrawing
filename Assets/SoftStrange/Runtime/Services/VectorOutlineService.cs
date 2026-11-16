using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace SoftStrange {
 [Serializable] public class BezierSegment {public Vector2 start,control1,control2,end;}
 [Serializable] public class BezierPath {public BezierSegment[] segments;}
 [Serializable] public class VectorDrawing {public string format="softstrange.bezier.v1";public int width,height;public bool truncated;public BezierPath[] paths;}
 public sealed class VectorOutlineService : IDisposable {
  const int MaxPaths=2048;
  readonly Transform root;
  readonly MeshFilter filter;
  readonly MeshRenderer renderer;
  Task<VectorDrawing> pending;
  int revision,pendingRevision;
  public VectorDrawing Drawing {get;private set;}
  public bool Busy=>pending!=null;
  public bool Visible {get=>renderer.enabled;set=>renderer.enabled=value&&Drawing!=null;}
  public Material Material=>renderer.sharedMaterial;
  public VectorOutlineService(Transform sheet,Material material){root=new GameObject("Bezier outlines").transform;root.SetParent(sheet,false);root.localPosition=new Vector3(0,0,-.003f);filter=root.gameObject.AddComponent<MeshFilter>();renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=new Material(material);renderer.enabled=false;}
  public void Clear(){revision++;pending=null;Drawing=null;renderer.enabled=false;}
  public void Begin(Texture2D image,float threshold=.4f,bool extractEdges=false){
   revision++;if(pending!=null){pending=null;}Drawing=null;renderer.enabled=false;
   var pixels=image.GetPixels32();int w=image.width,h=image.height;pendingRevision=revision;
   pending=Task.Run(()=>Trace(pixels,w,h,threshold,extractEdges));
  }
  public bool Tick(){if(pending==null||!pending.IsCompleted)return false;var task=pending;pending=null;if(task.IsFaulted){Debug.LogWarning("Curve conversion: "+task.Exception.GetBaseException().Message);return false;}if(pendingRevision!=revision)return false;Drawing=task.Result;if(Drawing.truncated)Debug.LogWarning("Curve conversion exceeded the path budget; retain the complete raster outline.");Debug.Log("SoftStrange curves ready: "+Drawing.paths.Length+" paths");return true;}
  public void Rebuild(float width){
   if(Drawing==null)return;var vertices=new List<Vector3>();var uvs=new List<Vector2>();var triangles=new List<int>();
   float aspect=(float)Drawing.height/Drawing.width;
   foreach(var path in Drawing.paths){float arc=0;foreach(var segment in path.segments){Vector2 previous=new Vector2(segment.start.x,segment.start.y*aspect);for(int i=1;i<=10;i++){float t=i/10f;Vector2 raw=Evaluate(segment,t);Vector2 p=new Vector2(raw.x,raw.y*aspect);Vector2 delta=p-previous;if(delta.sqrMagnitude<.00000001f)continue;Vector2 normal=new Vector2(-delta.y,delta.x).normalized*width*.5f;int k=vertices.Count;vertices.Add(ToPlane(previous-normal,aspect));vertices.Add(ToPlane(previous+normal,aspect));vertices.Add(ToPlane(p-normal,aspect));vertices.Add(ToPlane(p+normal,aspect));float end=arc+delta.magnitude;uvs.Add(new Vector2(arc,0));uvs.Add(new Vector2(arc,1));uvs.Add(new Vector2(end,0));uvs.Add(new Vector2(end,1));triangles.AddRange(new[]{k,k+1,k+2,k+2,k+1,k+3});arc=end;previous=p;}}}
   var mesh=new Mesh{name="Bezier stroke ribbons",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetUVs(0,uvs);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();if(filter.sharedMesh!=null)UnityEngine.Object.Destroy(filter.sharedMesh);filter.sharedMesh=mesh;
  }
  static Vector3 ToPlane(Vector2 point,float aspect)=>new Vector3(point.x-.5f,(point.y-aspect*.5f)/aspect,0);
  static Vector2 Evaluate(BezierSegment s,float t){float a=1-t;return a*a*a*s.start+3*a*a*t*s.control1+3*a*t*t*s.control2+t*t*t*s.end;}
  static VectorDrawing Trace(Color32[] source,int sourceW,int sourceH,float threshold,bool extractEdges){
   float scale=Math.Min(1,512f/Math.Max(sourceW,sourceH));int w=Math.Max(3,(int)(sourceW*scale)),h=Math.Max(3,(int)(sourceH*scale));var ink=new bool[w*h];
   var sampled=new Color32[w*h];
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){int sx=(int)(x/scale),sy=(int)(y/scale);Color32 sample=source[Math.Min(sourceH-1,sy)*sourceW+Math.Min(sourceW-1,sx)];if(!extractEdges){for(int yy=sy;yy<Math.Min(sourceH,(int)Math.Ceiling((y+1)/scale));yy++)for(int xx=sx;xx<Math.Min(sourceW,(int)Math.Ceiling((x+1)/scale));xx++)if(source[yy*sourceW+xx].a>sample.a)sample=source[yy*sourceW+xx];}sampled[y*w+x]=sample;}
   for(int y=1;y<h-1;y++)for(int x=1;x<w-1;x++){int p=y*w+x;if(!extractEdges)ink[p]=sampled[p].a/255f>threshold;else {float gx=-Luma(sampled[p-w-1])-2*Luma(sampled[p-1])-Luma(sampled[p+w-1])+Luma(sampled[p-w+1])+2*Luma(sampled[p+1])+Luma(sampled[p+w+1]);float gy=Luma(sampled[p-w-1])+2*Luma(sampled[p-w])+Luma(sampled[p-w+1])-Luma(sampled[p+w-1])-2*Luma(sampled[p+w])-Luma(sampled[p+w+1]);ink[p]=Math.Sqrt(gx*gx+gy*gy)>threshold;}}
   var remove=new List<int>();bool changed=true;int iterations=0;
   while(changed&&iterations++<40){changed=false;for(int phase=0;phase<2;phase++){
    remove.Clear();for(int y=1;y<h-1;y++)for(int x=1;x<w-1;x++){int p=y*w+x;if(!ink[p])continue;bool[] n={ink[p+w],ink[p+w+1],ink[p+1],ink[p-w+1],ink[p-w],ink[p-w-1],ink[p-1],ink[p+w-1]};int count=0,turns=0;for(int i=0;i<8;i++){if(n[i])count++;if(!n[i]&&n[(i+1)%8])turns++;}if(count<2||count>6||turns!=1)continue;if(phase==0?(n[0]&&n[2]&&n[4])||(n[2]&&n[4]&&n[6]):(n[0]&&n[2]&&n[6])||(n[0]&&n[4]&&n[6]))continue;remove.Add(p);}foreach(int p in remove)ink[p]=false;changed|=remove.Count>0;
   }}
   var neighbours=new Dictionary<int,List<int>>();for(int p=0;p<ink.Length;p++){if(!ink[p])continue;var list=new List<int>();int x=p%w,y=p/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dy==0)continue;int xx=x+dx,yy=y+dy;if(xx<0||yy<0||xx>=w||yy>=h)continue;int q=yy*w+xx;if(!ink[q])continue;if(dx!=0&&dy!=0&&(ink[y*w+xx]||ink[yy*w+x]))continue;list.Add(q);}neighbours[p]=list;}
   var visited=new HashSet<long>();var paths=new List<BezierPath>();var starts=new List<int>();foreach(var item in neighbours)if(item.Value.Count!=2)starts.Add(item.Key);foreach(var item in neighbours)if(item.Value.Count==2)starts.Add(item.Key);
   foreach(int start in starts){foreach(int first in neighbours[start]){if(paths.Count>=MaxPaths)break;long edge=Edge(start,first);if(visited.Contains(edge))continue;var points=new List<Vector2>{new Vector2((start%w+.5f)/w,(start/w+.5f)/h)};int previous=start,current=first;
    while(true){visited.Add(Edge(previous,current));points.Add(new Vector2((current%w+.5f)/w,(current/w+.5f)/h));var n=neighbours[current];if(n.Count!=2||current==start||points.Count>10000)break;int next=n[0]==previous?n[1]:n[0];if(visited.Contains(Edge(current,next)))break;previous=current;current=next;}
    if(points.Count<5)continue;var reduced=new List<Vector2>();Simplify(points,0,points.Count-1,.0018f,reduced);reduced.Add(points[points.Count-1]);if(reduced.Count<2)continue;
    var segments=new List<BezierSegment>();for(int i=0;i<reduced.Count-1;i++){Vector2 a=reduced[Math.Max(0,i-1)],b=reduced[i],c=reduced[i+1],d=reduced[Math.Min(reduced.Count-1,i+2)];segments.Add(new BezierSegment{start=b,control1=b+(c-a)/6,control2=c-(d-b)/6,end=c});}paths.Add(new BezierPath{segments=segments.ToArray()});
   }}return new VectorDrawing{width=sourceW,height=sourceH,truncated=paths.Count>=MaxPaths,paths=paths.ToArray()};
  }
  static float Luma(Color32 c)=>(.299f*c.r+.587f*c.g+.114f*c.b)/255f*c.a/255f;
  static long Edge(int a,int b){return ((long)Math.Min(a,b)<<32)|(uint)Math.Max(a,b);}
  static void Simplify(List<Vector2> points,int a,int b,float tolerance,List<Vector2> output){float max=0;int index=-1;Vector2 delta=points[b]-points[a];for(int i=a+1;i<b;i++){float t=delta.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(points[i]-points[a],delta)/delta.sqrMagnitude):0;float distance=(points[i]-(points[a]+delta*t)).magnitude;if(distance>max){max=distance;index=i;}}if(index>=0&&max>tolerance){Simplify(points,a,index,tolerance,output);Simplify(points,index,b,tolerance,output);}else output.Add(points[a]);}
  public string ExportJson()=>Drawing==null?"{}":JsonUtility.ToJson(Drawing,true);
  public void Dispose(){revision++;if(filter!=null&&filter.sharedMesh!=null)UnityEngine.Object.Destroy(filter.sharedMesh);if(renderer!=null)UnityEngine.Object.Destroy(renderer.sharedMaterial);if(root!=null)UnityEngine.Object.Destroy(root.gameObject);}
 }
}
