using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace SoftStrange {
 public interface IDrawingView:IDisposable {void Refresh();void Recenter();void SetHome(bool visible);void Tick();}
 public sealed class StudioView:IDrawingView {
  readonly DrawingApp app;
  readonly Transform frame,home,toolbar,drawer,debug;
  readonly TMP_Text title,subtitle,status,debugText,toast;
  string lastNotice;float noticeUntil;
  readonly RawImage preview;
  readonly PinchButton lockButton;
  readonly ReferenceModelService model;
  Transform content,dotControl;

  int page,category;
  float yaw,height;
  bool following;
  public string DrawerName {get;private set;}="";
  public bool DebugVisible=>debug.gameObject.activeSelf;
  public Vector3 FramePosition=>frame.position;
  public Quaternion FrameRotation=>frame.rotation;
  public bool BlockingDrawing=>app.HomeVisible||DrawerName!="";
  static readonly Color Cream=new Color(.96f,.92f,.81f),Sage=new Color(.27f,.40f,.34f,.92f),Surface=new Color(.055f,.10f,.09f,.78f);
  public StudioView(DrawingApp app){
   this.app=app;frame=new GameObject("Steady studio frame").transform;
   home=Panel("Welcome nook",new Vector2(670,500),new Vector3(0,.03f,0));
   title=Text(home,"Title",new Vector2(-48,195),new Vector2(500,50),34,"Soft Strange Drawing");
   Text(home,"Welcome",new Vector2(-40,153),new Vector2(500,32),18,"A little space to slow down and draw.");
   var card=SurfaceRect(home,"Reference card",new Vector2(0,30),new Vector2(580,180),new Color(.13f,.22f,.18f,.74f));
   preview=Rect(card,"Preview",Vector2.zero,new Vector2(400,170)).gameObject.AddComponent<RawImage>();preview.raycastTarget=false;
   subtitle=Text(home,"Subject",new Vector2(0,-80),new Vector2(580,35),22,"");
   Button(home,"Start drawing","draw",new Vector2(-142,-153),new Vector2(246,62),()=>app.Execute("draw"),true);
   Button(home,"Choose outline","library",new Vector2(142,-153),new Vector2(246,62),()=>OpenDrawer("library"));
   Text(home,"First steps",new Vector2(0,-214),new Vector2(580,32),16,"Pinch to place. Two hands to resize. Lock, then trace.");
   Circle(home,"Studio","info",new Vector2(-292,196),48,()=>OpenDrawer("studio"),false);
   toolbar=Panel("Drawing dock",new Vector2(470,146),new Vector3(0,-.23f,0));
   Circle(toolbar,"Outlines","library",new Vector2(-145,12),66,()=>OpenDrawer("library"));
   lockButton=Circle(toolbar,"Lock","lock",new Vector2(0,12),76,()=>{if(app.GridSelected)app.Execute("grid.select",flag:false);else app.Execute(app.Sheet.Locked?"unlock":"lock");});
   Circle(toolbar,"Palette","palette",new Vector2(145,12),66,()=>OpenDrawer("ink"));
   status=Text(toolbar,"Status",new Vector2(0,-58),new Vector2(440,28),15,"");
   drawer=Panel("Studio drawer",new Vector2(720,560),new Vector3(0,0,-.075f));drawer.gameObject.AddComponent<DrawerMotion>();
   Button(drawer,"Ink","palette",new Vector2(-235,240),new Vector2(140,42),()=>OpenDrawer("ink"));
   Button(drawer,"Grid","grid",new Vector2(-77,240),new Vector2(140,42),()=>OpenDrawer("grid"));
   Button(drawer,"Studio","sound",new Vector2(81,240),new Vector2(140,42),()=>OpenDrawer("studio"));
   Button(drawer,"Done","close",new Vector2(245,240),new Vector2(140,42),CloseDrawer);
   debug=Panel("Live state",new Vector2(340,230),new Vector3(.40f,0,0));debugText=Text(debug,"State",Vector2.zero,new Vector2(310,215),16,"");debug.gameObject.SetActive(false);drawer.gameObject.SetActive(false);
   var notification=Panel("Helpful hint",new Vector2(680,55),new Vector3(0,-.325f,-.09f));UnityEngine.Object.Destroy(notification.GetComponent<BoxCollider>());toast=Text(notification,"Message",Vector2.zero,new Vector2(650,50),17,"");notification.gameObject.SetActive(false);
   model=new ReferenceModelService(frame);Recenter();Refresh();
  }
  Transform Panel(string name,Vector2 size,Vector3 local){var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(Image));go.transform.SetParent(frame,false);go.transform.localPosition=local;go.transform.localScale=Vector3.one*.00085f;var rt=(RectTransform)go.transform;rt.sizeDelta=size;var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.overrideSorting=true;canvas.sortingOrder=20;canvas.worldCamera=app.View;var image=go.GetComponent<Image>();image.sprite=CozyGraphics.Round;image.type=Image.Type.Sliced;image.color=Surface;image.raycastTarget=false;go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit=12;
   // The backing intercepts rays so controls hidden behind a drawer never activate.
   go.layer=8;var blocker=go.AddComponent<BoxCollider>();blocker.size=new Vector3(size.x,size.y,2);blocker.center=new Vector3(0,0,4);return rt;}
  static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=position;return rt;}
  static Transform SurfaceRect(Transform parent,string name,Vector2 pos,Vector2 size,Color color){var rt=Rect(parent,name,pos,size);var image=rt.gameObject.AddComponent<Image>();image.sprite=CozyGraphics.Round;image.type=Image.Type.Sliced;image.color=color;image.raycastTarget=false;return rt;}
  TMP_Text Text(Transform parent,string name,Vector2 pos,Vector2 size,float font,string text){var rt=Rect(parent,name,pos,size);var t=rt.gameObject.AddComponent<TextMeshProUGUI>();t.font=app.Font;t.fontSize=font;t.text=text;t.color=Cream;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;}
  PinchButton Button(Transform parent,string label,string icon,Vector2 pos,Vector2 size,Action action,bool hero=false){var rt=Rect(parent,label,pos,size);rt.gameObject.layer=8;var image=rt.gameObject.AddComponent<Image>();image.sprite=CozyGraphics.Round;image.type=Image.Type.Sliced;image.color=hero?new Color(.43f,.49f,.30f,.96f):Sage;var button=rt.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;button.onClick.AddListener(()=>{if(!app.InputAllowed)return;app.Audio?.Cue("tap");action();});var collider=rt.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(size.x,size.y,16);var pinch=rt.gameObject.AddComponent<PinchButton>();pinch.Button=button;
   float offset=string.IsNullOrEmpty(icon)?0:16;pinch.Label=Text(rt,"Label",new Vector2(offset,0),new Vector2(size.x-2*offset-8,size.y-4),18,label);if(!string.IsNullOrEmpty(icon)){var ir=Rect(rt,"Icon",new Vector2(-size.x*.5f+25,0),new Vector2(28,28));pinch.Icon=ir.gameObject.AddComponent<CozyIcon>();pinch.Icon.Symbol=icon;pinch.Icon.color=Cream;pinch.Icon.raycastTarget=false;}return pinch;}
  PinchButton Circle(Transform parent,string label,string icon,Vector2 pos,float diameter,Action action,bool showLabel=true){var b=Button(parent,label,null,pos,new Vector2(diameter,diameter),action);b.Button.image.sprite=CozyGraphics.Circle;b.Button.image.type=Image.Type.Simple;UnityEngine.Object.Destroy(b.GetComponent<BoxCollider>());var collider=b.gameObject.AddComponent<SphereCollider>();collider.radius=diameter*.5f;b.Label.rectTransform.anchoredPosition=new Vector2(0,-diameter*.5f-15);b.Label.rectTransform.sizeDelta=new Vector2(140,28);b.Label.fontSize=17;b.Label.gameObject.SetActive(showLabel);var ir=Rect(b.transform,"Icon",Vector2.zero,new Vector2(diameter*.56f,diameter*.56f));b.Icon=ir.gameObject.AddComponent<CozyIcon>();b.Icon.Symbol=icon;b.Icon.color=Cream;b.Icon.raycastTarget=false;return b;}
  Transform Slider(Transform parent,string name,float y,float min,float max,Func<float> read,Action<float> write){var rt=Rect(parent,name,new Vector2(0,y),new Vector2(630,48));Text(rt,"Label",new Vector2(-235,0),new Vector2(165,40),18,name);var track=SurfaceRect(rt,"Track",new Vector2(75,0),new Vector2(405,25),new Color(.32f,.41f,.32f,.9f));track.gameObject.layer=8;var collider=track.gameObject.AddComponent<BoxCollider>();collider.size=new Vector3(405,44,16);var slider=track.gameObject.AddComponent<PinchSlider>();slider.Initialize(min,max,read,write);var knob=Rect(track,"Handle",Vector2.zero,new Vector2(25,35));var image=knob.gameObject.AddComponent<Image>();image.sprite=CozyGraphics.Round;image.color=new Color(.94f,.78f,.49f);slider.Handle=knob;return rt;}
  public void OpenDrawer(string kind){if(!app.InputAllowed)return;DrawerName=kind;app.CancelInteractions();home.gameObject.SetActive(false);toolbar.gameObject.SetActive(false);drawer.gameObject.SetActive(true);BuildDrawer();}
  public void CloseDrawer(){DrawerName="";drawer.gameObject.SetActive(false);if(content!=null)content.gameObject.SetActive(false);app.Images.ReleaseThumbnails();home.gameObject.SetActive(app.HomeVisible);toolbar.gameObject.SetActive(!app.HomeVisible);Refresh();}
  void BuildDrawer(){if(content!=null){content.gameObject.SetActive(false);UnityEngine.Object.Destroy(content.gameObject);}app.Images.ReleaseThumbnails();content=Rect(drawer,"Drawer contents",new Vector2(0,-18),new Vector2(680,450));dotControl=null;
   switch(DrawerName){case "library":Library();break;case "grid":Grid();break;case "studio":Studio();break;case "privacy":Privacy();break;default:Ink();break;}
  }
  void Ink(){
   Text(content,"Heading",new Vector2(0,194),new Vector2(620,38),24,"Make the lines your own");
   Button(content,"Solid","solid",new Vector2(-160,140),new Vector2(285,45),()=>app.Execute("style.dotted",flag:false));Button(content,"Dotted","dotted",new Vector2(160,140),new Vector2(285,45),()=>app.Execute("style.dotted",flag:true));
   Slider(content,"Opacity",77,.1f,1,()=>app.Opacity,v=>app.Execute("style.opacity",v));Slider(content,"Line width",19,.001f,.014f,()=>app.LineWidth,v=>app.Execute("style.width",v));
   dotControl=Slider(content,"Dot spacing",-39,.009f,.07f,()=>app.DotSpacing,v=>app.Execute("style.dotSpacing",v));
   if(app.EdgeMode)Slider(content,"Edge detail",-97,.02f,.65f,()=>app.Threshold,v=>app.Execute("style.threshold",v));
   string[] colors={"#FFFFFF","#171B18","#F3CD80","#A6D8BD","#E5B3B9","#AAC6E2"};for(int i=0;i<colors.Length;i++){string c=colors[i];var swatch=Button(content,"",null,new Vector2(-240+i*96,-159),new Vector2(60,46),()=>app.Execute("style.color",text:c));ColorUtility.TryParseHtmlString(c,out var col);swatch.Button.image.color=col;}
   Button(content,"Bring here","recenter",new Vector2(-163,-218),new Vector2(290,42),()=>{app.Execute("recenter");CloseDrawer();});if(app.Images.HasPair)Button(content,"Side / top",null,new Vector2(163,-218),new Vector2(290,42),()=>{if(app.Images.HasPair)app.Execute("image.view",text:app.Images.Perspective=="side"?"top":"side");});
  }
  void Grid(){
   Text(content,"Heading",new Vector2(0,190),new Vector2(630,45),25,"A steady guide for your page");
   Text(content,"Explanation",new Vector2(0,125),new Vector2(620,65),18,"The grid stays where you place it, even when you move or lock your outline.");
   Button(content,app.Guides.Visible?"Hide grid":"Show grid","grid",new Vector2(-160,48),new Vector2(285,55),()=>{app.Execute("guides.visible",flag:!app.Guides.Visible);BuildDrawer();});
   Button(content,app.GridSelected?"Finish placement":"Move grid","draw",new Vector2(160,48),new Vector2(285,55),()=>{app.Execute("grid.select",flag:!app.GridSelected);CloseDrawer();});
   Button(content,"Align to outline","recenter",new Vector2(0,-26),new Vector2(610,48),()=>app.Execute("grid.align"));
   Slider(content,"Grid size",-106,.15f,1.3f,()=>app.Guides.Sheet.transform.localScale.x,v=>app.Execute("grid.width",v));
   Text(content,"Help",new Vector2(0,-183),new Vector2(615,65),18,"Choose Move grid, then pinch to place it. Tap Done on the drawing dock when it feels right.");
  }
  void Studio(){
   Text(content,"Heading",new Vector2(0,190),new Vector2(630,45),25,"Settle into your drawing nook");
   Button(content,app.Audio.Muted?"Sound off":"Sound on","sound",new Vector2(-160,120),new Vector2(285,48),()=>{app.Execute("audio.muted",flag:!app.Audio.Muted);BuildDrawer();});
   Button(content,app.ReducedMotion?"Still details":"Gentle motion",null,new Vector2(160,120),new Vector2(285,48),()=>{app.Execute("comfort.reducedMotion",flag:!app.ReducedMotion);BuildDrawer();});
   Slider(content,"Volume",52,0,1,()=>app.Audio.Volume,v=>app.Execute("audio.volume",v));
   Button(content,"Bring menu here","recenter",new Vector2(-160,-12),new Vector2(285,48),Recenter);Button(content,"Home","home",new Vector2(160,-12),new Vector2(285,48),()=>app.Execute("home"));
   Text(content,"Help",new Vector2(0,-90),new Vector2(620,90),17,"Hands: aim, then pinch thumb and index.\nPinch near your outline to move it; use two hands to resize.\nControllers: trigger selects; grip holds the outline.");
   Button(content,"Privacy & credits","info",new Vector2(-160,-180),new Vector2(285,46),()=>OpenDrawer("privacy"));Button(content,"Live frame rate",null,new Vector2(160,-180),new Vector2(285,46),()=>app.Execute("debug.panel"));
   Text(content,"Build",new Vector2(0,-230),new Vector2(630,24),12,"Soft Strange Drawing  "+Application.version+"  ·  Original models and sounds");
  }
  void Privacy(){Text(content,"Heading",new Vector2(0,190),new Vector2(630,45),25,"Your quiet, private studio");Text(content,"Privacy",new Vector2(0,0),new Vector2(615,320),18,"Your images stay in this app's storage on this headset.\nNo account, advertising, analytics uploads, or cloud service.\n\nHand poses are used to aim and place your outline.\nPassthrough is composited by the headset; this app does not read camera images.\n\nLocal diagnostic logs are bounded. Review builds can record poses for debugging. Uninstalling removes local app data.\n\nArtwork: filtered AI illustrations and original 3D studies.\nCreative drawing references, not anatomical diagrams.\nAudio: original synthesized tones. Font: Liberation Sans (OFL).\nUnity / OpenXR: see bundled third-party notices.");Button(content,"Back to studio","back",new Vector2(0,-220),new Vector2(300,45),()=>OpenDrawer("studio"));}
  void Library(){
   var indices=new List<int>();for(int i=0;i<app.Images.Count;i++){bool pair=app.Images.HasPairAt(i),imported=app.Images.IsImportedAt(i),shark=app.Images.IsSharkAt(i);if(category==0&&app.Images.IsAIAt(i)||category==1&&pair||category==2&&shark||category==3&&!shark&&!imported||category==4&&imported)indices.Add(i);}
   int pages=Mathf.Max(1,(indices.Count+3)/4);page=Mathf.Clamp(page,0,pages-1);
   string[] cats={"AI art","Side / top","Sharks","Nature","My images"};for(int c=0;c<5;c++){int selected=c;Button(content,cats[c],null,new Vector2(-260+c*130,184),new Vector2(122,36),()=>{category=selected;page=0;BuildDrawer();});}
   for(int i=0;i<4;i++){
    int slot=page*4+i;if(slot>=indices.Count)break;int index=indices[slot];var card=SurfaceRect(content,"Subject "+index,new Vector2(i%2==0?-163:163,i<2?72:-90),new Vector2(308,151),new Color(.13f,.21f,.17f,.86f));var name=Text(card,"Name",new Vector2(0,55),new Vector2(290,30),17,app.Images.NameAt(index));name.enableAutoSizing=true;name.fontSizeMin=11;name.fontSizeMax=17;
    ViewChoice(card,index,"side",app.Images.HasPairAt(index)?-73:0);if(app.Images.HasPairAt(index))ViewChoice(card,index,"top",73);
   }
   Button(content,"Previous","back",new Vector2(-234,-213),new Vector2(180,44),()=>{page=(page+pages-1)%pages;BuildDrawer();});Button(content,"Import","import",new Vector2(0,-213),new Vector2(180,44),()=>{CloseDrawer();app.Execute("image.import");});Button(content,"Next","next",new Vector2(234,-213),new Vector2(180,44),()=>{page=(page+1)%pages;BuildDrawer();});Text(content,"Page",new Vector2(0,-254),new Vector2(630,26),13,$"{page+1} / {pages}   ·   Pick a view and make it yours");
  }
  void ViewChoice(Transform card,int index,string view,float x){var b=Button(card,"",null,new Vector2(x,-12),new Vector2(138,105),()=>{app.Execute("image.select",index,text:view);CloseDrawer();});b.Button.image.color=new Color(.18f,.28f,.23f,.72f);var raw=Rect(b.transform,"Outline",new Vector2(0,9),new Vector2(123,75)).gameObject.AddComponent<RawImage>();raw.texture=app.Images.GetThumbnail(index,view);raw.raycastTarget=false;Fit(raw,new Vector2(123,75));Text(b.transform,"View",new Vector2(0,-39),new Vector2(125,24),14,app.Images.HasPairAt(index)?view=="side"?"Side":"Top":"Use image");}
  static void Fit(RawImage raw,Vector2 box){if(raw.texture==null)return;float ratio=(float)raw.texture.width/raw.texture.height;raw.rectTransform.sizeDelta=ratio>box.x/box.y?new Vector2(box.x,box.x/ratio):new Vector2(box.y*ratio,box.y);}
  public void SetHome(bool visible){CloseDrawer();home.gameObject.SetActive(visible);toolbar.gameObject.SetActive(!visible);}
  public void Refresh(){if(app.Images.Texture==null)return;preview.texture=app.Images.Texture;Fit(preview,new Vector2(500,170));subtitle.text=app.Images.Name+(app.Images.HasPair?" · "+app.Images.Perspective:"");model.Select(app.Images.ModelPath);lockButton.Label.text=app.GridSelected?"Done":app.Sheet.Locked?"Unlock":"Lock";lockButton.Icon.Symbol=app.GridSelected?"draw":app.Sheet.Locked?"lock":"unlock";lockButton.Icon.SetVerticesDirty();status.text=app.GridSelected?"Moving grid · pinch near the guide":app.Sheet.Locked?"Locked in place. Take your time.":"Pinch to place · two hands to resize";if(!app.InputAllowed)status.text="Paused while system menu is open";else if(Application.isMobilePlatform&&!app.Left.Tracked&&!app.Right.Tracked)status.text="Bring your hands into view, or pick up controllers";if(dotControl!=null)dotControl.gameObject.SetActive(app.Dotted);var d=app.Services.Get<DiagnosticsService>();debugText.text=$"{d.Fps:F0} FPS · {(d.Fps>0?1000/d.Fps:0):0.0} ms\nL {(app.Left.Tracked?"tracked":"waiting")} · R {(app.Right.Tracked?"tracked":"waiting")}\n{app.Images.Name}\n{(app.GridSelected?"Grid selected":app.Sheet.Locked?"Locked":"Positioning")}\n{(app.Vectors.Busy?"Preparing curves…":app.Vectors.Drawing!=null?"Curves ready":"Outline ready")}\n{app.Notice}";}
  public void ToggleDebug(){debug.gameObject.SetActive(!debug.gameObject.activeSelf);Refresh();}
  public void Recenter(){var camera=app.View.transform;Vector3 forward=Vector3.ProjectOnPlane(camera.forward,Vector3.up);if(forward.sqrMagnitude<.01f)forward=Vector3.forward;yaw=Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg;height=camera.position.y-.08f;frame.rotation=Quaternion.Euler(0,yaw,0);frame.position=new Vector3(camera.position.x,height,camera.position.z)+frame.forward*.80f;following=false;}
  public void Tick(){if(lastNotice!=app.Notice){lastNotice=app.Notice;noticeUntil=Time.unscaledTime+5;toast.text=lastNotice;}toast.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(lastNotice)&&Time.unscaledTime<noticeUntil);float dt=Mathf.Min(Time.unscaledDeltaTime,.1f);var head=app.View.transform;bool interacting=app.Left.PinchStrength>.2f||app.Right.PinchStrength>.2f||app.ActiveSheet.Busy;Vector3 forward=Vector3.ProjectOnPlane(head.forward,Vector3.up);if(!interacting&&forward.sqrMagnitude>.1f){float target=Mathf.Atan2(forward.x,forward.z)*Mathf.Rad2Deg;float error=Mathf.Abs(Mathf.DeltaAngle(yaw,target));if(error>30)following=true;if(error<3)following=false;if(following)yaw=Mathf.LerpAngle(yaw,target,1-Mathf.Exp(-dt/1.1f));frame.rotation=Quaternion.Euler(0,yaw,0);if(Mathf.Abs(head.position.y-.08f-height)>.30f)height=Mathf.Lerp(height,head.position.y-.08f,1-Mathf.Exp(-dt/1.4f));Vector3 destination=new Vector3(head.position.x,height,head.position.z)+frame.forward*.80f;frame.position=Vector3.Lerp(frame.position,destination,1-Mathf.Exp(-dt/.45f));}model.Tick(app.InputAllowed?dt:0,app.HomeVisible&&DrawerName=="",app.ReducedMotion);}
  public void Dispose(){app.Images.ReleaseThumbnails();model.Dispose();if(frame!=null)UnityEngine.Object.Destroy(frame.gameObject);}
 }
 public sealed class PinchSlider:MonoBehaviour {
  public RectTransform Handle;
  float min,max,lastWrite=-10;Func<float> read;Action<float> write;HandInput active;
  public static PinchSlider CapturedLeft,CapturedRight;
  public void Initialize(float minimum,float maximum,Func<float> get,Action<float> set){min=minimum;max=maximum;read=get;write=set;}
  public void Drag(HandInput hand){if(hand.Pressed)active=hand;if(active==hand&&hand.Pinching)SetFromRay(hand.Ray);}
  public void SetFromRay(Ray ray){if(Time.unscaledTime-lastWrite<.04f)return;var plane=new Plane(transform.forward,transform.position);if(plane.Raycast(ray,out float distance)){var local=transform.InverseTransformPoint(ray.GetPoint(distance));float t=Mathf.Clamp01(local.x/((RectTransform)transform).sizeDelta.x+.5f);float value=Mathf.Lerp(min,max,t);if(Mathf.Abs(value-read())>(max-min)*.004f){lastWrite=Time.unscaledTime;write(value);}}}
  void Update(){if(Handle!=null&&read!=null)Handle.anchoredPosition=new Vector2((Mathf.InverseLerp(min,max,read())-.5f)*((RectTransform)transform).sizeDelta.x,0);if(active!=null){if(!active.Pinching||!active.Tracked)active=null;else SetFromRay(active.Ray);}}
  void OnDisable(){active=null;}
 }
}
