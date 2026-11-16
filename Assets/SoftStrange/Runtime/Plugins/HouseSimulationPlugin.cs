using UnityEngine;
namespace SoftStrange {
 public sealed class HouseSimulationPlugin : IAppPlugin {
  DrawingApp app;
  GameObject house;
  float time;
  bool replay;
  Vector3 start;
  Quaternion rotation;
  public string Id=>"house-simulation";
  public bool Visible=>house!=null&&house.activeSelf;
  public void Initialize(DrawingApp owner,ServiceRegistry services){
   app=owner;if(!Application.isMobilePlatform)LoadHouse();
   services.Get<CommandService>().Register("simulation.house","Show or hide the authored house",c=>{if(c.flag&&house==null)LoadHouse();if(house!=null)house.SetActive(c.flag);});
   services.Get<CommandService>().Register("simulation.play","Run a deterministic placement and drawing scenario",c=>{if(house==null)LoadHouse();if(house!=null)house.SetActive(true);app.ResetPlacement();start=app.Sheet.transform.position;rotation=app.Sheet.transform.rotation;time=0;replay=true;app.Sheet.Locked=false;});
   services.Get<CommandService>().Register("simulation.stop","Stop the user scenario",c=>replay=false);
  }
  void LoadHouse(){
   var prefab=Resources.Load<GameObject>("DrawingHouse");if(prefab!=null){house=Object.Instantiate(prefab);house.name="Drawing house simulation";house.SetActive(!Application.isMobilePlatform);if(!Application.isMobilePlatform){Transform eye=null,desk=null;foreach(var node in house.GetComponentsInChildren<Transform>()){if(node.name=="UserEye")eye=node;if(node.name=="DrawingDeskAnchor")desk=node;}if(eye!=null){app.View.transform.position=eye.position;if(desk!=null)app.View.transform.LookAt(new Vector3(desk.position.x,eye.position.y,desk.position.z));}}}
  }
  public void Tick(float dt){if(!replay)return;time+=dt;
   if(time<3){app.Sheet.transform.position=start+new Vector3(Mathf.Sin(time)*.04f,-time*.035f,0);app.Sheet.transform.rotation=rotation*Quaternion.Euler(time*8,0,time*4);}
   else if(time<6){float scale=.4f+.04f*Mathf.Sin((time-3)*1.1f);float aspect=(float)app.Images.Texture.height/app.Images.Texture.width;app.Sheet.transform.localScale=new Vector3(scale,scale*aspect,1);}
   else if(time<8){app.Sheet.Locked=true;}
   else {replay=false;Debug.Log("SoftStrange simulation: placement, rotation, resize and lock complete");}
  }
  public void Dispose(){if(house!=null)Object.Destroy(house);}
 }
}
