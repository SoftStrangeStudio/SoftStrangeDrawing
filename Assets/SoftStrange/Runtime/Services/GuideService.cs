using UnityEngine;
namespace SoftStrange {
 public sealed class GuideService : System.IDisposable {
  readonly Transform root;
  readonly Material material;
  public ReferenceSheet Sheet {get;}
  public bool Placed {get;private set;}
  public bool Visible {get=>root.gameObject.activeSelf;set{root.gameObject.SetActive(value);if(!value)Sheet.Cancel();}}
  public bool Selected {get;private set;}
  public GuideService(Transform reference,Material source){
   root=new GameObject("Independent drawing grid").transform;
   material=new Material(Resources.Load<Material>("Pointer")){color=new Color(.85f,.77f,.58f,.28f)};
   Sheet=root.gameObject.AddComponent<ReferenceSheet>();Sheet.Locked=true;
   for(int i=0;i<=8;i++){float p=-.5f+i*.125f;Line(new Vector3(p,-.5f,0),new Vector3(p,.5f,0));Line(new Vector3(-.5f,p,0),new Vector3(.5f,p,0));}
   Visible=false;
  }
  void Line(Vector3 a,Vector3 b){var line=new GameObject("Grid line").AddComponent<LineRenderer>();line.transform.SetParent(root,false);line.useWorldSpace=false;line.sharedMaterial=material;line.positionCount=2;line.SetPosition(0,a);line.SetPosition(1,b);line.startWidth=line.endWidth=.0015f;}
  public void Place(Transform reference){root.SetPositionAndRotation(reference.position+reference.forward*.004f,reference.rotation);root.localScale=new Vector3(reference.localScale.x,reference.localScale.y,1);Placed=true;}
  public void Select(bool selected){Selected=selected;Sheet.Cancel();Sheet.Locked=!selected;material.color=selected?new Color(.9f,.79f,.55f,.65f):new Color(.85f,.77f,.58f,.28f);}
  public void Tick(){Sheet.Tick();}
  public void Dispose(){Object.Destroy(material);if(root!=null)Object.Destroy(root.gameObject);}
 }
}
