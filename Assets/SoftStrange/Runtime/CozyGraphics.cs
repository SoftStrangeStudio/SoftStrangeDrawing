using UnityEngine;
using UnityEngine.UI;
namespace SoftStrange {
 public static class CozyGraphics {
  static Sprite round,circle;
  public static Sprite Round=>round??(round=Make(false));
  public static Sprite Circle=>circle??(circle=Make(true));
  static Sprite Make(bool disk){const int n=64;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.name=disk?"Circle":"Rounded surface";var pixels=new Color32[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){float dx=Mathf.Abs(x-31.5f),dy=Mathf.Abs(y-31.5f);float d=disk?Mathf.Sqrt(dx*dx+dy*dy)-30:Mathf.Sqrt(Mathf.Pow(Mathf.Max(dx-18,0),2)+Mathf.Pow(Mathf.Max(dy-18,0),2))-13;pixels[y*n+x]=new Color32(255,255,255,(byte)(Mathf.Clamp01(.5f-d)*255));}t.SetPixels32(pixels);t.Apply();return Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(20,20,20,20));}
 }
 public sealed class CozyIcon : MaskableGraphic {
  public string Symbol;
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();float s=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);System.Action<Vector2,Vector2> line=(a,b)=>{a*=s;b*=s;Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*s*.032f;int k=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);};
   System.Action<Vector2,float,int> arc=(center,r,steps)=>{for(int i=0;i<steps;i++){float a=i*Mathf.PI*2/steps,b=(i+1)*Mathf.PI*2/steps;line(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r);}};
   if(Symbol=="lock"||Symbol=="unlock"){line(new Vector2(-.28f,-.30f),new Vector2(.28f,-.30f));line(new Vector2(-.28f,.08f),new Vector2(.28f,.08f));line(new Vector2(-.28f,-.30f),new Vector2(-.28f,.08f));line(new Vector2(.28f,-.30f),new Vector2(.28f,.08f));for(int i=0;i<12;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;line(new Vector2(Mathf.Cos(a)*.18f+(Symbol=="unlock"?.18f:0),.1f+Mathf.Sin(a)*.24f),new Vector2(Mathf.Cos(b)*.18f+(Symbol=="unlock"?.18f:0),.1f+Mathf.Sin(b)*.24f));}line(new Vector2(0,-.05f),new Vector2(0,-.18f));}
   else if(Symbol=="palette"){arc(Vector2.zero,.32f,30);arc(new Vector2(-.12f,.12f),.035f,8);arc(new Vector2(.12f,.12f),.035f,8);arc(new Vector2(-.14f,-.1f),.035f,8);arc(new Vector2(.1f,-.14f),.055f,8);}
   else if(Symbol=="grid"){for(int i=-1;i<=1;i++){float p=i*.24f;line(new Vector2(p,-.32f),new Vector2(p,.32f));line(new Vector2(-.32f,p),new Vector2(.32f,p));}}
   else if(Symbol=="draw"){line(new Vector2(-.23f,-.24f),new Vector2(.22f,.25f));line(new Vector2(-.14f,-.30f),new Vector2(.30f,.18f));line(new Vector2(-.23f,-.24f),new Vector2(-.28f,-.35f));line(new Vector2(-.28f,-.35f),new Vector2(-.14f,-.30f));line(new Vector2(.22f,.25f),new Vector2(.30f,.18f));}
   else if(Symbol=="close"){line(new Vector2(-.23f,-.23f),new Vector2(.23f,.23f));line(new Vector2(-.23f,.23f),new Vector2(.23f,-.23f));}
   else if(Symbol=="next"||Symbol=="back"){float sign=Symbol=="next"?1:-1;line(new Vector2(-sign*.13f,.26f),new Vector2(sign*.13f,0));line(new Vector2(sign*.13f,0),new Vector2(-sign*.13f,-.26f));}
   else if(Symbol=="recenter"){arc(Vector2.zero,.24f,24);line(new Vector2(-.4f,0),new Vector2(.4f,0));line(new Vector2(0,-.4f),new Vector2(0,.4f));}
   else if(Symbol=="sound"){line(new Vector2(-.3f,-.12f),new Vector2(-.3f,.12f));line(new Vector2(-.3f,.12f),new Vector2(.03f,.30f));line(new Vector2(.03f,.30f),new Vector2(.03f,-.30f));line(new Vector2(.03f,-.30f),new Vector2(-.3f,-.12f));for(int i=0;i<10;i++){float a=-.9f+i*.18f,b=a+.18f;line(new Vector2(.05f+Mathf.Cos(a)*.3f,Mathf.Sin(a)*.3f),new Vector2(.05f+Mathf.Cos(b)*.3f,Mathf.Sin(b)*.3f));}}
   else if(Symbol=="home"){line(new Vector2(-.35f,0),new Vector2(0,.3f));line(new Vector2(0,.3f),new Vector2(.35f,0));line(new Vector2(-.23f,0),new Vector2(-.23f,-.3f));line(new Vector2(-.23f,-.3f),new Vector2(.23f,-.3f));line(new Vector2(.23f,-.3f),new Vector2(.23f,0));}
   else if(Symbol=="library"){for(int i=0;i<3;i++){float x=-.28f+i*.25f;line(new Vector2(x,-.28f),new Vector2(x,.28f));line(new Vector2(x,.28f),new Vector2(x+.13f,.28f));line(new Vector2(x+.13f,.28f),new Vector2(x+.13f,-.28f));line(new Vector2(x+.13f,-.28f),new Vector2(x,-.28f));}}
   else if(Symbol=="import"){line(new Vector2(0,-.03f),new Vector2(0,.34f));line(new Vector2(0,.34f),new Vector2(-.14f,.18f));line(new Vector2(0,.34f),new Vector2(.14f,.18f));line(new Vector2(-.29f,.0f),new Vector2(-.29f,-.26f));line(new Vector2(-.29f,-.26f),new Vector2(.29f,-.26f));line(new Vector2(.29f,-.26f),new Vector2(.29f,0));}
   else if(Symbol=="solid")line(new Vector2(-.35f,0),new Vector2(.35f,0));
   else if(Symbol=="dotted")for(int i=0;i<5;i++)arc(new Vector2(-.3f+i*.15f,0),.025f,10);
   else {arc(Vector2.zero,.29f,24);line(new Vector2(0,.14f),new Vector2(0,-.08f));arc(new Vector2(0,-.19f),.012f,8);}
  }
 }
 public sealed class DrawerMotion : MonoBehaviour {
  CanvasGroup group;float time;Vector3 size;
  void Awake(){group=gameObject.AddComponent<CanvasGroup>();size=transform.localScale;}
  void OnEnable(){time=0;}
  void Update(){time+=Time.unscaledDeltaTime;float t=PlayerPrefs.GetInt("comfort.reducedMotion",0)==1?1:Mathf.SmoothStep(0,1,time/.18f);group.alpha=t;transform.localScale=size*Mathf.Lerp(.975f,1,t);}
 }
}
