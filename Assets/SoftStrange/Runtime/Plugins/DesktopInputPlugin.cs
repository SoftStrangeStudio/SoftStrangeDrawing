using UnityEngine;
using UnityEngine.InputSystem;
namespace SoftStrange {
 public sealed class DesktopInputPlugin : IAppPlugin {
  DrawingApp app;
  Vector3 offset;
  bool grabbed;
  public string Id=>"desktop-input";
  public void Initialize(DrawingApp owner,ServiceRegistry services){app=owner;}
  public void Tick(float dt){
   var keyboard=Keyboard.current;var mouse=Mouse.current;if(keyboard==null||mouse==null||!app.InputAllowed)return;
   if(keyboard.rKey.wasPressedThisFrame)app.Execute("recenter");
   if(keyboard.spaceKey.wasPressedThisFrame)app.Execute(app.Sheet.Locked?"unlock":"lock");
   if(keyboard.rightArrowKey.wasPressedThisFrame)app.Execute("image.next");
   if(keyboard.leftArrowKey.wasPressedThisFrame)app.Execute("image.previous");
   if(keyboard.f1Key.wasPressedThisFrame)app.Execute("debug.panel");
   if(keyboard.escapeKey.wasPressedThisFrame)app.Execute("home");
   if(mouse.rightButton.isPressed){Vector2 delta=mouse.delta.ReadValue();app.View.transform.Rotate(Vector3.up,delta.x*.10f,Space.World);app.View.transform.Rotate(Vector3.right,-delta.y*.10f,Space.Self);}
   if(keyboard.wKey.isPressed)app.View.transform.position+=app.View.transform.forward*dt;
   if(keyboard.sKey.isPressed)app.View.transform.position-=app.View.transform.forward*dt;
   if(keyboard.aKey.isPressed)app.View.transform.position-=app.View.transform.right*dt;
   if(keyboard.dKey.isPressed)app.View.transform.position+=app.View.transform.right*dt;
   Ray ray=app.View.ScreenPointToRay(mouse.position.ReadValue());var plane=new Plane(app.ActiveSheet.transform.forward,app.ActiveSheet.transform.position);
   bool ui=Physics.Raycast(ray,out var hit,5,1<<8);
   if(ui&&mouse.leftButton.isPressed){var slider=hit.collider.GetComponent<PinchSlider>();if(slider!=null)slider.SetFromRay(ray);}
   if(ui){var hover=hit.collider.GetComponent<PinchButton>();hover?.Hover(mouse.leftButton.isPressed?1:.3f);}
   if(mouse.leftButton.wasPressedThisFrame&&ui){var button=hit.collider.GetComponent<PinchButton>();if(button!=null)button.Press();}
   if(app.CanManipulate&&!app.ActiveSheet.Locked&&!ui&&plane.Raycast(ray,out float distance)){
    Vector3 point=ray.GetPoint(distance);
    if(mouse.leftButton.wasPressedThisFrame&&app.ActiveSheet.Near(point)){grabbed=true;offset=app.ActiveSheet.transform.position-point;}
    if(grabbed&&mouse.leftButton.isPressed)app.ActiveSheet.transform.position=point+offset;
    float scroll=mouse.scroll.ReadValue().y;if(Mathf.Abs(scroll)>0){float size=Mathf.Clamp(app.ActiveSheet.transform.localScale.x*Mathf.Exp(scroll*.001f),.1f,2);float aspect=app.ActiveSheet.transform.localScale.y/app.ActiveSheet.transform.localScale.x;app.ActiveSheet.transform.localScale=new Vector3(size,size*aspect,1);}
   }
   if(mouse.leftButton.wasReleasedThisFrame||!app.CanManipulate||app.ActiveSheet.Locked)grabbed=false;
  }
  public void Dispose(){}
 }
}
