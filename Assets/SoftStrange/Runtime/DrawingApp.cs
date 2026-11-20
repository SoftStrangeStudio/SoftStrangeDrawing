using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.XR;
using TMPro;

namespace SoftStrange {
 public sealed class DrawingApp : MonoBehaviour {
  public Camera View;
  public Transform Origin;
  public Material SheetMaterial,HandMaterial;
  public TMP_FontAsset Font;
  public ReferenceSheet Sheet {get;private set;}
  public HandInput Left {get;private set;}
  public HandInput Right {get;private set;}
  public ImageLibraryService Images {get;private set;}
  public readonly ServiceRegistry Services=new ServiceRegistry();
  public bool SimulatedInput;
  public bool ShowSkeleton;
  public bool PreferControllers;
  public bool ReducedMotion {get;private set;}
  public bool InputAllowed=>!paused&&(!Application.isMobilePlatform||(focused&&xrFocused));
  public GuideService Guides=>guides;
  public bool GridSelected=>guides!=null&&guides.Selected;
  public ReferenceSheet ActiveSheet=>GridSelected?guides.Sheet:Sheet;
  public bool CanManipulate=>InputAllowed&&!HomeVisible&&(ui==null||!ui.BlockingDrawing);
  public ComfortAudioService Audio {get;private set;}
  bool paused,focused=true,xrFocused=true,menuHeld;
  XRDisplaySubsystem display;
  readonly List<XRDisplaySubsystem> displays=new List<XRDisplaySubsystem>();
  readonly Collider[] nearControls=new Collider[32];
  readonly RaycastHit[] magneticHits=new RaycastHit[32];
  sealed class UiSelection {
   public PinchButton hover;
   public float acquiredAt,lastSeenAt;
   public bool consumed;
  }
  readonly UiSelection leftUi=new UiSelection(),rightUi=new UiSelection();
  const float HoverRetention=.18f,PressHistory=.10f,TargetStability=.06f;
  const float MagnetAngle=1.5f,NearRadius=.018f;
  void ClearUi(UiSelection state){
   if(state.hover!=null)state.hover.ClearTarget();
   state.hover=null;state.acquiredAt=state.lastSeenAt=0;state.consumed=false;
  }
  public bool HomeVisible {get;private set;}=true;
  public float Opacity {get;private set;}=.85f;
  public float Threshold {get;private set;}=.18f;
  public float LineWidth {get;private set;}=.004f;
  public Color Ink {get;private set;}=Color.white;
  public bool Dotted {get;private set;}
  public float DotSpacing {get;private set;}=.012f;
  public float DotSize {get;private set;}=.45f;
  public bool EdgeMode {get;private set;}
  public bool CurveMode {get;private set;}
  public string Notice {get;private set;}="Choose an outline, place it, then lock it to draw.";
  public VectorOutlineService Vectors {get;private set;}
  CommandService commands;
  DiagnosticsService diagnostics;
  GuideService guides;
  StudioView ui;
  UploadServer endpoint;
  XRHandSubsystem hands;
  readonly List<XRHandSubsystem> subsystems=new List<XRHandSubsystem>();
  readonly List<IAppPlugin> plugins=new List<IAppPlugin>();
  float nextSnapshot,nextInbox;
  string lastError;
  void Start(){
   Application.targetFrameRate=72;
   diagnostics=new DiagnosticsService();diagnostics.Record(Debug.isDebugBuild);commands=new CommandService();
   ReducedMotion=PlayerPrefs.GetInt("comfort.reducedMotion",0)==1;Opacity=PlayerPrefs.GetFloat("style.opacity",.85f);Dotted=PlayerPrefs.GetInt("style.dotted",0)==1;LineWidth=PlayerPrefs.GetFloat("style.width",.004f);DotSpacing=PlayerPrefs.GetFloat("style.dotSpacing",.012f);DotSize=PlayerPrefs.GetFloat("style.dotSize",.45f);ColorUtility.TryParseHtmlString(PlayerPrefs.GetString("style.color","#FFFFFF"),out var savedInk);Ink=savedInk;
   Audio=new ComfortAudioService(View);Services.Register(Audio);
   Services.Register(diagnostics);Services.Register(commands);
   var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Reference sheet";Destroy(quad.GetComponent<MeshCollider>());
   quad.GetComponent<Renderer>().sharedMaterial=new Material(SheetMaterial);Sheet=quad.AddComponent<ReferenceSheet>();Sheet.Material=quad.GetComponent<Renderer>().sharedMaterial;
   Left=new HandInput(transform,HandMaterial);Right=new HandInput(transform,HandMaterial);
   Images=new ImageLibraryService();Services.Register(Images);Services.Register(Sheet);
   guides=new GuideService(Sheet.transform,HandMaterial);Services.Register(guides);
   Vectors=new VectorOutlineService(Sheet.transform,Resources.Load<Material>("Curves"));Services.Register(Vectors);
   RegisterCommands();
   if(!Application.isMobilePlatform){
    foreach(var session in FindObjectsByType<ARSession>(FindObjectsSortMode.None))session.enabled=false;
    if(View.TryGetComponent<ARCameraManager>(out var cameraManager))cameraManager.enabled=false;
    if(View.TryGetComponent<TrackedPoseDriver>(out var poseDriver))poseDriver.enabled=false;
    View.clearFlags=CameraClearFlags.SolidColor;View.backgroundColor=new Color(.075f,.105f,.11f);
    AddPlugin(new DesktopInputPlugin());
   }
   AddPlugin(new HouseSimulationPlugin());
   ui=new StudioView(this);Images.Changed+=ImageChanged;ImageChanged();ResetPlacement();SetHome(true);
   if(Debug.isDebugBuild||!Application.isMobilePlatform){endpoint=new UploadServer(Resources.Load<TextAsset>("Uploader").text);endpoint.Commands=JsonUtility.ToJson(new CommandList{commands=commands.Descriptions.ToArray()});endpoint.Start();}PublishState();
   Debug.Log("SoftStrange ready: "+Images.Count+" offline references, platform="+Application.platform);
  }
  void AddPlugin(IAppPlugin plugin){plugin.Initialize(this,Services);plugins.Add(plugin);}
  void Update(){
   if(Sheet==null)return;
   float dt=Time.unscaledDeltaTime;diagnostics.Tick(dt);
   if(display==null&&Application.isMobilePlatform){displays.Clear();SubsystemManager.GetSubsystems(displays);display=displays.Find(d=>d.running);if(display!=null)display.displayFocusChanged+=DisplayFocus;}
   if(InputAllowed){
    foreach(var plugin in plugins)plugin.Tick(dt);
    if(!SimulatedInput){
     if(Application.isMobilePlatform){
      if(hands==null||!hands.running){subsystems.Clear();SubsystemManager.GetSubsystems(subsystems);hands=subsystems.Find(h=>h.running);}
      ReadInput(Left,true);ReadInput(Right,false);
      var leftDevice=InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);leftDevice.TryGetFeatureValue(CommonUsages.menuButton,out bool menu);if(menu&&!menuHeld)SetHome(true);menuHeld=menu;
     }else {Left.LoseTracking();Right.LoseTracking();}
    }
    Interact(Left);Interact(Right);Sheet.Tick();guides.Tick();
   }else {CancelInteractions();Left.LoseTracking();Right.LoseTracking();}
   if(Vectors.Tick()){if(Vectors.Drawing.truncated){CurveMode=false;Dotted=false;Notice="This image has too much fine detail for curves. Keeping the complete solid outline.";}else Vectors.Rebuild(LineWidth);Vectors.Visible=CurveMode;Sheet.GetComponent<Renderer>().enabled=!CurveMode;ApplyStyle();}
   if(endpoint!=null){
    while(endpoint.CommandQueue.TryDequeue(out var request)){
     try{if(!InputAllowed)throw new InvalidOperationException("Application is paused by the system");var command=JsonUtility.FromJson<AppCommand>(request.Json);commands.Execute(command);SavePreferences();PublishState();request.Result="{\"ok\":true,\"state\":"+diagnostics.Json+"}";}
     catch(Exception e){lastError=e.Message;request.Result="{\"ok\":false,\"error\":\""+Escape(e.Message)+"\"}";}
     request.Completed.Set();
    }
    if(endpoint.Take(out var bytes)){try{Images.Import(bytes);SetHome(false);Notice="Image received. Position it, then lock to draw.";}catch(Exception e){lastError=e.Message;Notice=e.Message;}}
   }
   if(InputAllowed&&Time.unscaledTime>=nextInbox){nextInbox=Time.unscaledTime+2;int before=Images.Count;Images.ScanInbox();if(Images.Count>before){SetHome(false);Notice="Your image is ready to draw.";}}
   if(Time.unscaledTime>=nextSnapshot){nextSnapshot=Time.unscaledTime+.5f;PublishState();ui?.Refresh();}
   ui?.Tick();
  }
  void ReadInput(HandInput input,bool left){
   var controller=InputDevices.GetDeviceAtXRNode(left?XRNode.LeftHand:XRNode.RightHand);
   if(PreferControllers&&controller.isValid&&controller.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)&&tracked){input.ReadController(controller,Origin);return;}
   var aim=left?MetaAimHand.left:MetaAimHand.right;bool reserved=aim!=null&&(((MetaAimFlags)aim.aimFlags.ReadValue()&(MetaAimFlags.SystemGesture|MetaAimFlags.MenuPressed))!=0);
   if(hands!=null&&hands.running&&(left?hands.leftHand.isTracked:hands.rightHand.isTracked))input.Read(left?hands.leftHand:hands.rightHand,Origin,ShowSkeleton,reserved);
   else input.ReadController(controller,Origin);
  }
  bool Eligible(PinchButton button)=>button!=null&&button.CanAcquire;
  // Exact hits and occluding panels always take precedence over magnetic assistance.
  PinchButton FindTarget(HandInput hand,out Vector3 end,out bool blocked,out PinchSlider slider){
   end=hand.Ray.GetPoint(.85f);blocked=false;slider=null;
   int nearby=Physics.OverlapSphereNonAlloc(hand.Point,NearRadius,nearControls,1<<8);
   PinchButton near=null;float nearDistance=float.MaxValue;
   for(int i=0;i<nearby;i++){
    var button=nearControls[i].GetComponent<PinchButton>();
    if(!Eligible(button))continue;
    float d=nearControls[i].ClosestPoint(hand.Point).sqrMagnitude;
    if(d<nearDistance){nearDistance=d;near=button;}
   }
   if(near!=null){end=near.transform.position;return near;}
   if(Physics.Raycast(hand.Ray,out var exact,2f,1<<8)){
    blocked=true;end=exact.point;
    slider=exact.collider.GetComponent<PinchSlider>();
    if(slider!=null)return null;
    var direct=exact.collider.GetComponent<PinchButton>();
    return Eligible(direct)?direct:null;
   }
   // Angular assistance grows with distance; do not activate through other UI surfaces.
   float radius=Mathf.Tan(MagnetAngle*Mathf.Deg2Rad)*.85f;
   int count=Physics.SphereCastNonAlloc(hand.Ray,radius,magneticHits,2f,1<<8);
   PinchButton best=null;float score=float.MaxValue;
   for(int i=0;i<count;i++){
    var hit=magneticHits[i];
    var button=hit.collider.GetComponent<PinchButton>();
    if(!Eligible(button))continue;
    Vector3 aim=hit.collider.ClosestPoint(hand.Ray.GetPoint(Mathf.Clamp(hit.distance,0,2)));
    Vector3 delta=aim-hand.Ray.origin;
    if(delta.sqrMagnitude<.0001f)continue;
    float angle=Vector3.Angle(hand.Ray.direction,delta);
    if(angle>MagnetAngle)continue;
    // A panel or another control in front must prevent acquisition.
    if(Physics.Raycast(hand.Ray.origin,delta.normalized,out var visibility,delta.magnitude+.005f,1<<8)
       &&visibility.collider!=hit.collider)continue;
    float rank=angle+hit.distance*.001f;
    if(rank<score){score=rank;best=button;end=aim;}
   }
   return best;
  }
  void Interact(HandInput hand){
   var state=hand==Left?leftUi:rightUi;
   if(!hand.Tracked||!InputAllowed){ClearUi(state);hand.UiTargeted=false;return;}
   var candidate=FindTarget(hand,out var end,out var blocked,out var slider);
   if(slider!=null){
    if(state.hover!=null)state.hover.ClearTarget();
    state.hover=null;hand.UiTargeted=false;slider.Drag(hand);hand.ShowPointer(end);
    return;
   }
   float now=Time.unscaledTime;
   if(candidate!=null){
    if(candidate!=state.hover){
     if(state.hover!=null)state.hover.ClearTarget();
     state.hover=candidate;state.acquiredAt=now;
    }
    state.lastSeenAt=now;
   }else if(state.hover!=null){
    // Retain a recently aimed target only when it remains active and nearby.
    Vector3 toward=state.hover.transform.position-hand.Ray.origin;
    bool close=toward.sqrMagnitude>.0001f&&Vector3.Angle(hand.Ray.direction,toward)<3f;
    if(blocked||!Eligible(state.hover)||!close||now-state.lastSeenAt>HoverRetention){
     state.hover.ClearTarget();state.hover=null;
    }
   }
   var target=state.hover;
   hand.UiTargeted=target!=null;
   if(target!=null){
    bool stable=now-state.acquiredAt>=TargetStability;
    target.SetTargeted(stable,Mathf.Max(.25f,hand.PinchStrength));
    end=target.transform.position;
    // A target may be selected from the recent hover history when pinch movement shifts aim.
    if(hand.Pressed&&!state.consumed&&now-state.lastSeenAt<=PressHistory&&Eligible(target)){
     state.consumed=true;
     target.Press();
    }
   }
   hand.ShowPointer(end);
   if(hand.Released)state.consumed=false;
   if(target==null&&!blocked&&hand.GrabPressed&&CanManipulate)ActiveSheet.TryGrab(hand);
  }
  public void CancelInteractions(){
   Sheet?.Cancel();guides?.Sheet.Cancel();
   ClearUi(leftUi);ClearUi(rightUi);
   if(Left!=null)Left.UiTargeted=false;
   if(Right!=null)Right.UiTargeted=false;
  }
  public void Execute(string name,float value=0,string text=null,bool flag=false,Vector3 vector=default){try{if(!InputAllowed)return;commands.Execute(new AppCommand{name=name,value=value,text=text,flag=flag,vector=vector,id=Guid.NewGuid().ToString("N")});if(name=="lock"||name=="draw"||name=="image.select")Audio?.Cue(name);SavePreferences();PublishState();ui?.Refresh();}catch(Exception e){Notice=lastError=e.Message;Debug.LogWarning("Command: "+e.Message);}}
  void RegisterCommands(){
   commands.Register("app.quit","Quit the local application",c=>Application.Quit());
   commands.Register("home","Open the reference home screen",c=>SetHome(true));
   commands.Register("draw","Use the selected reference",c=>{SetHome(false);ResetPlacement();});
   commands.Register("recenter","Bring the outline and menu here without moving the grid",c=>StartCoroutine(Recenter()));
   commands.Register("lock","Lock the reference for physical drawing",c=>{CancelInteractions();guides.Select(false);Sheet.Locked=true;Notice="Locked. Take your time and draw.";});
   commands.Register("unlock","Unlock for positioning",c=>{Sheet.Locked=false;Notice="Pinch near the outline to position it.";});
   commands.Register("image.next","Select the next offline or imported reference",c=>Images.Select(Images.Index+1));
   commands.Register("image.previous","Select the previous reference",c=>Images.Select(Images.Index-1));
   commands.Register("image.select","Select an image by zero-based index",c=>{Images.Select((int)c.value,c.text);SetHome(false);ResetPlacement();});
   commands.Register("image.view","Choose side or top for the current subject",c=>Images.Select(Images.Index,c.text));
   commands.Register("image.import","Open the headset file picker",c=>ImportImage());
   commands.Register("sheet.visible","Show/hide the drawing plane",c=>Sheet.gameObject.SetActive(c.flag));
   commands.Register("sheet.position","Set the plane position",c=>{Sheet.Cancel();Sheet.transform.position=c.vector;});
   commands.Register("sheet.rotation","Set plane Euler angles",c=>{Sheet.Cancel();Sheet.transform.rotation=Quaternion.Euler(c.vector);});
   commands.Register("sheet.width","Set width in metres",c=>{float width=Mathf.Clamp(c.value,.1f,2);Sheet.transform.localScale=new Vector3(width,width*Images.Texture.height/Images.Texture.width,1);});
   commands.Register("guides.visible","Show/hide alignment grid",c=>{if(c.flag&&!guides.Placed)guides.Place(Sheet.transform);guides.Visible=c.flag;if(!c.flag)guides.Select(false);});
   commands.Register("grid.select","Select grid for independent manipulation",c=>{CancelInteractions();if(c.flag){SetHome(false);if(!guides.Placed)guides.Place(Sheet.transform);guides.Visible=true;}guides.Select(c.flag);});
   commands.Register("grid.align","Align grid to the current reference",c=>{guides.Sheet.Cancel();guides.Place(Sheet.transform);guides.Visible=true;});
   commands.Register("grid.position","Place grid independently in world space",c=>{guides.Sheet.Cancel();guides.Sheet.transform.position=c.vector;});
   commands.Register("grid.rotation","Set independent grid rotation",c=>{guides.Sheet.Cancel();guides.Sheet.transform.rotation=Quaternion.Euler(c.vector);});
   commands.Register("grid.width","Set independent grid width in metres",c=>{guides.Sheet.Cancel();var t=guides.Sheet.transform;float ratio=t.localScale.y/Mathf.Max(.001f,t.localScale.x);float w=Mathf.Clamp(c.value,.1f,2);t.localScale=new Vector3(w,w*ratio,1);});
   commands.Register("audio.muted","Mute/unmute all application audio",c=>Audio.SetMuted(c.flag));
   commands.Register("audio.volume","Set audio volume",c=>Audio.SetVolume(c.value));
   commands.Register("comfort.reducedMotion","Reduce decorative motion",c=>{ReducedMotion=c.flag;PlayerPrefs.SetInt("comfort.reducedMotion",c.flag?1:0);});
   commands.Register("menu.open","Open ink, library, grid, studio, or privacy drawer",c=>ui.OpenDrawer(c.text??"ink"));
   commands.Register("menu.close","Close the current drawer",c=>ui.CloseDrawer());
   commands.Register("style.opacity","Set line opacity from 0 to 1",c=>{Opacity=Mathf.Clamp01(c.value);ApplyStyle();});
   commands.Register("style.color","Set a line color using hex RGB/RGBA",c=>{if(!ColorUtility.TryParseHtmlString(c.text,out var color))throw new ArgumentException("Use a hex color such as #FFFFFF");Ink=color;ApplyStyle();});
   commands.Register("style.width","Set normalized curve width",c=>{LineWidth=Mathf.Clamp(c.value,.0005f,.025f);if(!CurveMode)ConvertCurves();else Vectors.Rebuild(LineWidth);ApplyStyle();});
   commands.Register("style.threshold","Set edge sensitivity",c=>{Threshold=Mathf.Clamp(c.value,.01f,1);if(CurveMode&&EdgeMode)ConvertCurves();ApplyStyle();});
   commands.Register("style.dotted","Choose dotted or solid strokes",c=>{Dotted=c.flag;if(Dotted&&!CurveMode)ConvertCurves();ApplyStyle();});
   commands.Register("style.dotSpacing","Set normalized dot spacing",c=>{DotSpacing=Mathf.Clamp(c.value,.003f,.15f);ApplyStyle();});
   commands.Register("style.dotSize","Set dot size from 0.1 to 0.5",c=>{DotSize=Mathf.Clamp(c.value,.1f,.5f);ApplyStyle();});
   commands.Register("style.edges","Switch imported photos to Sobel edges",c=>{EdgeMode=c.flag;CurveMode=false;Vectors.Visible=false;Sheet.GetComponent<Renderer>().enabled=true;ApplyStyle();});
   commands.Register("vectors.convert","Trace centerlines and fit cubic Bezier paths",c=>ConvertCurves());
   commands.Register("vectors.export","Save normalized Bezier control points and paths",c=>{if(Vectors.Drawing==null)throw new InvalidOperationException("Convert the outline first");string folder=Path.Combine(Application.persistentDataPath,"Exports");Directory.CreateDirectory(folder);string file=Path.Combine(folder,"reference-"+Images.Index+".json");File.WriteAllText(file,Vectors.ExportJson());Notice="Curve data saved in Exports.";});
   commands.Register("debug.panel","Toggle the floating frame/state panel",c=>ui?.ToggleDebug());
   commands.Register("debug.record","Start/stop local state recording",c=>diagnostics.Record(c.flag));
   commands.Register("hands.skeleton","Show/hide tracked or simulated hand bones",c=>ShowSkeleton=c.flag);
   commands.Register("capture.start","Capture a timed frame sequence",c=>StartCoroutine(Capture(Mathf.Clamp(c.value,1,30))));
  }
  void ConvertCurves(){CurveMode=true;Vectors.Begin(Images.Texture,EdgeMode?Threshold:.4f,EdgeMode);Notice="Converting strokes to smooth curve paths…";}
  void ApplyStyle(){
   Sheet.Material.SetFloat("_Opacity",Opacity);Sheet.Material.SetFloat("_Outline",EdgeMode?1:0);Sheet.Material.SetFloat("_Threshold",Threshold);Sheet.Material.SetColor("_Ink",Ink);Sheet.Material.SetFloat("_Tint",1);
   var material=Vectors.Material;material.SetColor("_Ink",Ink);material.SetFloat("_Opacity",Opacity);material.SetFloat("_Dotted",Dotted?1:0);material.SetFloat("_DotSpacing",DotSpacing);material.SetFloat("_DotSize",DotSize);material.SetFloat("_LineWidth",LineWidth);
  }
  void ImageChanged(){Sheet.Cancel();Vectors.Clear();EdgeMode=!Images.IsLineArt;Sheet.Material.mainTexture=Images.Texture;var scale=Sheet.transform.localScale;scale.y=scale.x*Images.Texture.height/Images.Texture.width;Sheet.transform.localScale=scale;CurveMode=false;Vectors.Visible=false;Sheet.GetComponent<Renderer>().enabled=true;if(Dotted)ConvertCurves();ApplyStyle();ui?.Refresh();}
  public void SetHome(bool visible){HomeVisible=visible;CancelInteractions();guides.Select(false);Sheet.gameObject.SetActive(!visible);ui?.SetHome(visible);}
  public void ResetPlacement(){
   CancelInteractions();guides.Select(false);Vector3 forward=View.transform.forward;Sheet.transform.SetPositionAndRotation(View.transform.position+forward*.60f,View.transform.rotation);Sheet.transform.localScale=new Vector3(.40f,.40f*Images.Texture.height/Images.Texture.width,1);Sheet.Locked=false;if(!guides.Placed)guides.Place(Sheet.transform);ui?.Recenter();
  }
  IEnumerator Recenter(){yield return null;ResetPlacement();Notice="Your outline and menu are here. The grid stays placed.";}
  IEnumerator Capture(float duration){string folder=Path.Combine(Application.persistentDataPath,"Captures",DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(folder);float started=Time.unscaledTime;int frame=0;while(Time.unscaledTime-started<duration){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,$"frame-{frame++:D5}.png"));yield return new WaitForSecondsRealtime(1f/24);}Notice="Capture saved: "+folder;Debug.Log("SoftStrange capture: "+folder);}
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
  [System.Runtime.InteropServices.DllImport("NativeReferencePicker")] static extern IntPtr SoftStrangePickImage();
  [System.Runtime.InteropServices.DllImport("NativeReferencePicker")] static extern void SoftStrangeFreeString(IntPtr pointer);
#endif
  public void ImportImage(){
#if UNITY_ANDROID && !UNITY_EDITOR
   using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=unity.GetStatic<AndroidJavaObject>("currentActivity"))using(var picker=new AndroidJavaClass("com.softstrange.drawing.ReferencePicker"))picker.CallStatic("pick",activity);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
   IntPtr result=SoftStrangePickImage();if(result!=IntPtr.Zero){string path=System.Runtime.InteropServices.Marshal.PtrToStringUTF8(result);SoftStrangeFreeString(result);try{Images.Import(File.ReadAllBytes(path));SetHome(false);}catch(Exception e){Notice=e.Message;}}
#else
   Notice="Drop an image into the uploader or copy it into the app Inbox.";
#endif
  }
  public void ImportFile(string path){try{if(path.StartsWith("ERROR:")){Notice=path.Substring(6);return;}string full=Path.GetFullPath(path);if(!full.StartsWith(Path.GetFullPath(Application.temporaryCachePath)+Path.DirectorySeparatorChar))throw new IOException("Invalid import location");try{Images.Import(File.ReadAllBytes(full));SetHome(false);}finally{File.Delete(full);}}catch(Exception e){Notice=lastError=e.Message;}}
  void PublishState(){if(diagnostics==null||Images==null)return;var state=new RuntimeState{version=Application.version,platform=Application.platform.ToString(),inputMode=SimulatedInput?"simulation":Application.isMobilePlatform?"XR hands":"desktop",reference=Images.Name,frame=Time.frameCount,seconds=Time.unscaledTime,imageIndex=Images.Index,imageCount=Images.Count,uploadCount=Images.UploadCount,xrRunning=hands!=null&&hands.running,leftTracked=Left.Tracked,rightTracked=Right.Tracked,leftPinching=Left.Pinching,rightPinching=Right.Pinching,locked=Sheet.Locked,visible=Sheet.gameObject.activeSelf,guides=guides.Visible,debugPanel=ui!=null&&ui.DebugVisible,simulation=SimulatedInput,inputAllowed=InputAllowed,gridSelected=GridSelected,gridPosition=guides.Sheet.transform.position,gridScale=guides.Sheet.transform.localScale,gridRotation=guides.Sheet.transform.rotation,menuPosition=ui!=null?ui.FramePosition:Vector3.zero,menuRotation=ui!=null?ui.FrameRotation:Quaternion.identity,drawer=ui?.DrawerName,perspective=Images.Perspective,audioMuted=Audio.Muted,leftPinchStrength=Left.PinchStrength,rightPinchStrength=Right.PinchStrength,headPosition=View.transform.position,sheetPosition=Sheet.transform.position,sheetRotation=Sheet.transform.rotation,sheetScale=Sheet.transform.localScale,leftPinchPosition=Left.Point,rightPinchPosition=Right.Point,leftRayDirection=Left.Ray.direction,rightRayDirection=Right.Ray.direction,home=HomeVisible,dotted=Dotted,curvesReady=Vectors.Drawing!=null,curvesBusy=Vectors.Busy,curvePaths=Vectors.Drawing!=null?Vectors.Drawing.paths.Length:0,color="#"+ColorUtility.ToHtmlStringRGBA(Ink),lineWidth=LineWidth,dotSpacing=DotSpacing,dotSize=DotSize,opacity=Opacity,threshold=Threshold,lastCommand=commands.LastName,lastCommandId=commands.LastId,lastError=lastError};diagnostics.Publish(state);if(endpoint!=null){endpoint.Snapshot=diagnostics.Json;endpoint.Events=diagnostics.EventsJson;}}
  public string StateJson=>diagnostics?.Json??"{}";
  static string Escape(string text)=>text.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n");
  [Serializable] class CommandList {public CommandDescription[] commands;}
  void SavePreferences(){if(Sheet==null)return;PlayerPrefs.SetFloat("style.opacity",Opacity);PlayerPrefs.SetFloat("style.width",LineWidth);PlayerPrefs.SetFloat("style.dotSpacing",DotSpacing);PlayerPrefs.SetFloat("style.dotSize",DotSize);PlayerPrefs.SetString("style.color","#"+ColorUtility.ToHtmlStringRGBA(Ink));PlayerPrefs.SetInt("style.dotted",Dotted?1:0);}
  void DisplayFocus(bool value){xrFocused=value;ApplyFocus();}
  void OnApplicationFocus(bool value){focused=value;ApplyFocus();}
  void OnApplicationPause(bool value){paused=value;ApplyFocus();if(value){SavePreferences();PlayerPrefs.Save();}}
  void ApplyFocus(){AudioListener.pause=paused||!focused||!xrFocused;if(!InputAllowed){CancelInteractions();Left?.LoseTracking();Right?.LoseTracking();}if(ui!=null)PublishState();Debug.Log("SoftStrange lifecycle: input="+InputAllowed+" audioPaused="+AudioListener.pause);}
  void OnDestroy(){if(display!=null)display.displayFocusChanged-=DisplayFocus;endpoint?.Dispose();foreach(var plugin in plugins)plugin.Dispose();ui?.Dispose();Images?.Dispose();Vectors?.Dispose();guides?.Dispose();diagnostics?.Dispose();Audio?.Dispose();Left?.Dispose();Right?.Dispose();AudioListener.pause=false;}
 }
}
