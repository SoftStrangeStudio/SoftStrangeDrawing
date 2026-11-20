using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
namespace SoftStrange {
 public sealed class HandInput : System.IDisposable {
  public bool Tracked,Pinching,Pressed,Released,GrabPressed,GrabHeld,Controller,SystemGesture;
  public float PinchStrength,PinchDistance;
  public float PinchStartedAt {get;private set;}=-1f;
  public Ray RawRay {get;private set;}
  public bool UiTargeted {get;set;}
  public Vector3 Point;
  public Quaternion Rotation;
  public Ray Ray;
  public readonly Pose[] Joints=new Pose[26];
  public readonly bool[] Valid=new bool[26];
  readonly Transform[] dots=new Transform[26];
  readonly LineRenderer[] bones=new LineRenderer[26];
  readonly LineRenderer pointer;
  readonly Transform reticle;
  readonly Material rayMaterial;
  bool armed,smoothReady,controllerArmed;
  float nearTime;
  Vector3 smoothOrigin,smoothDirection;
  static readonly int[] Parent={-1,0,0,2,3,4,0,6,7,8,9,0,11,12,13,14,0,16,17,18,19,0,21,22,23,24};
  public HandInput(Transform root,Material material){
   for(int i=0;i<26;i++){
    var dot=GameObject.CreatePrimitive(PrimitiveType.Sphere);Object.Destroy(dot.GetComponent<SphereCollider>());dot.name="Hand joint "+i;dot.transform.SetParent(root);dot.transform.localScale=Vector3.one*.004f;dot.GetComponent<Renderer>().sharedMaterial=material;dots[i]=dot.transform;dot.SetActive(false);
    var go=new GameObject("Hand bone "+i);go.transform.SetParent(root);bones[i]=go.AddComponent<LineRenderer>();bones[i].sharedMaterial=material;bones[i].positionCount=2;bones[i].startWidth=bones[i].endWidth=.0015f;bones[i].enabled=false;
   }
   rayMaterial=new Material(Resources.Load<Material>("Pointer"));
   pointer=new GameObject("Pinch pointer").AddComponent<LineRenderer>();pointer.transform.SetParent(root);pointer.sharedMaterial=rayMaterial;pointer.positionCount=2;pointer.startWidth=.0009f;pointer.endWidth=.0004f;pointer.numCapVertices=3;pointer.enabled=false;
   var marker=GameObject.CreatePrimitive(PrimitiveType.Sphere);
   marker.name="UI aim reticle";Object.Destroy(marker.GetComponent<SphereCollider>());
   marker.transform.SetParent(root);marker.transform.localScale=Vector3.one*.009f;
   marker.GetComponent<Renderer>().sharedMaterial=rayMaterial;marker.SetActive(false);reticle=marker.transform;
  }
  public void Read(XRHand hand,Transform origin,bool skeleton,bool systemGesture=false){
   if(Controller)LoseTracking();Controller=false;SystemGesture=systemGesture;
   bool previous=Pinching;
   for(int i=0;i<26;i++){var joint=hand.GetJoint((XRHandJointID)(i+1));Valid[i]=hand.isTracked&&joint.TryGetPose(out Joints[i]);if(Valid[i])Joints[i]=new Pose(origin.TransformPoint(Joints[i].position),origin.rotation*Joints[i].rotation);dots[i].gameObject.SetActive(skeleton&&Valid[i]&&!systemGesture);if(Valid[i])dots[i].SetPositionAndRotation(Joints[i].position,Joints[i].rotation);}
   Tracked=Valid[0]&&Valid[5]&&Valid[10]&&!systemGesture;
   if(Tracked){
    Point=(Joints[5].position+Joints[10].position)*.5f;Rotation=Joints[0].rotation;
    PinchDistance=Vector3.Distance(Joints[5].position,Joints[10].position);
    // Scale gently with hand size, with bounded values for stable acquisition.
    float size=Valid[11]?Vector3.Distance(Joints[0].position,Joints[11].position):.09f;
    float enter=Mathf.Clamp(size*.31f,.024f,.030f),exit=enter+.014f;
    if(PinchDistance>exit){armed=true;nearTime=0;}
    nearTime=PinchDistance<enter?nearTime+Time.unscaledDeltaTime:0;
    Pinching=armed&&(previous?PinchDistance<exit:nearTime>=.025f);
    PinchStrength=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.085f,enter,PinchDistance));
    // Aim exclusively from wrist orientation; finger curl only controls the pinch.
    SmoothRay(Rotation*Vector3.forward,UiTargeted?.11f:.075f);ShowPointer(Ray.GetPoint(.85f));
   }else {Pinching=false;armed=false;smoothReady=false;nearTime=0;PinchStrength=0;pointer.enabled=false;reticle.gameObject.SetActive(false);UiTargeted=false;}
   Pressed=Pinching&&!previous;Released=!Pinching&&previous;GrabPressed=Pressed;GrabHeld=Pinching;if(Pressed)PinchStartedAt=Time.unscaledTime;
   for(int i=0;i<26;i++){int p=Parent[i];bones[i].enabled=skeleton&&Tracked&&Valid[i]&&p>=0&&Valid[p];if(bones[i].enabled){bones[i].SetPosition(0,Joints[p].position);bones[i].SetPosition(1,Joints[i].position);}}
  }
  public void ReadController(InputDevice device,Transform origin){
   if(!Controller)LoseTracking();Controller=true;
   if(!device.isValid||!device.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)||!tracked||!device.TryGetFeatureValue(CommonUsages.devicePosition,out var position)||!device.TryGetFeatureValue(CommonUsages.deviceRotation,out var rotation)){LoseTracking();return;}
   bool before=Pinching,grabBefore=GrabHeld;device.TryGetFeatureValue(CommonUsages.trigger,out float trigger);device.TryGetFeatureValue(CommonUsages.grip,out float grip);
   if(trigger<.2f&&grip<.2f)controllerArmed=true;
   Tracked=true;Point=origin.TransformPoint(position);Rotation=origin.rotation*rotation;Pinching=controllerArmed&&trigger>(before?.35f:.65f);GrabHeld=controllerArmed&&grip>(grabBefore?.35f:.65f);Pressed=Pinching&&!before;Released=!Pinching&&before;GrabPressed=GrabHeld&&!grabBefore;PinchStrength=Mathf.Max(.25f,trigger);if(Pressed)PinchStartedAt=Time.unscaledTime;SmoothRay(Rotation*Vector3.forward);ShowPointer(Ray.GetPoint(.85f));
  }
  void SmoothRay(Vector3 direction,float directionSmoothing=.045f){if(direction.sqrMagnitude<.000001f)direction=Rotation*Vector3.forward;direction.Normalize();float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);if(!smoothReady){smoothOrigin=Point;smoothDirection=direction;smoothReady=true;}else{smoothOrigin=Vector3.Lerp(smoothOrigin,Point,1-Mathf.Exp(-dt/.025f));smoothDirection=Vector3.Slerp(smoothDirection,direction,1-Mathf.Exp(-dt/directionSmoothing));}RawRay=new Ray(Point,direction);Ray=new Ray(smoothOrigin,smoothDirection.normalized);}
  public void ShowPointer(Vector3 end){pointer.enabled=Tracked;rayMaterial.color=new Color(.76f,.91f,.84f,UiTargeted?.85f:.35f);pointer.SetPosition(0,Ray.origin);pointer.SetPosition(1,end);reticle.gameObject.SetActive(Tracked);if(Tracked)reticle.position=end;}
  public void LoseTracking(){Released=Pinching;Pinching=Pressed=GrabPressed=GrabHeld=Tracked=armed=smoothReady=controllerArmed=false;PinchStrength=0;nearTime=0;PinchStartedAt=-1f;UiTargeted=false;reticle.gameObject.SetActive(false);foreach(var t in dots)t.gameObject.SetActive(false);foreach(var b in bones)b.enabled=false;pointer.enabled=false;}
  public void Dispose(){Object.Destroy(rayMaterial);if(pointer!=null)Object.Destroy(pointer.gameObject);if(reticle!=null)Object.Destroy(reticle.gameObject);foreach(var t in dots)if(t!=null)Object.Destroy(t.gameObject);foreach(var b in bones)if(b!=null)Object.Destroy(b.gameObject);}
 }
}
