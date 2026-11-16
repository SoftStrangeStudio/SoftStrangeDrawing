using UnityEngine;
namespace SoftStrange {
 public sealed class ReferenceModelService : System.IDisposable {
  readonly Transform root;
  GameObject model;
  Material material;
  string path;
  float phase;
  public ReferenceModelService(Transform frame){root=new GameObject("Little studio sculpture").transform;root.SetParent(frame,false);root.localPosition=new Vector3(.23f,.19f,-.065f);material=new Material(Resources.Load<Material>("Sculpture"));material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.045f,.085f,.06f));}
  public void Select(string resource){if(resource==path)return;path=resource;if(model!=null)Object.Destroy(model);if(string.IsNullOrEmpty(resource))return;var prefab=Resources.Load<GameObject>(resource);if(prefab==null)return;model=Object.Instantiate(prefab,root);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=new Bounds(model.transform.position,Vector3.zero);bool first=true;foreach(var r in renderers){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}float scale=.145f/Mathf.Max(.001f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));model.transform.localScale*=scale;model.transform.position-=bounds.center*scale-root.position*scale;}
  public void Tick(float dt,bool visible,bool reducedMotion){root.gameObject.SetActive(visible);if(!visible)return;phase+=dt;if(reducedMotion){root.localRotation=Quaternion.Euler(0,15,0);root.localPosition=new Vector3(.23f,.19f,-.065f);}else{root.localRotation=Quaternion.Euler(0,15+Mathf.Sin(phase*.45f)*12,Mathf.Sin(phase*.6f)*2);root.localPosition=new Vector3(.23f,.19f+Mathf.Sin(phase*.7f)*.006f,-.065f);}}
  public void Dispose(){if(root!=null)Object.Destroy(root.gameObject);Object.Destroy(material);}
 }
}
