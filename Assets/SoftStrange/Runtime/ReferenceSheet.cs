using UnityEngine;

namespace SoftStrange {
 public sealed class ReferenceSheet : MonoBehaviour {
  public bool Locked;
  public Material Material;
  HandInput first,second;
  Vector3 startPosition,startScale,offset,startMid,startAxis;
  Quaternion startRotation,startHandRotation;
  float startDistance;
  public bool Busy => first!=null;
  public void Cancel() {first=second=null;}
  public bool Near(Vector3 point) {
   Vector3 p=transform.InverseTransformPoint(point);
   return Mathf.Abs(p.z)*transform.lossyScale.z<.10f&&Mathf.Abs(p.x)<.56f&&Mathf.Abs(p.y)<.56f;
  }
  public void TryGrab(HandInput hand) {
   if(!gameObject.activeSelf||Locked||!hand.GrabPressed||!Near(hand.Point)||first==hand||second==hand)return;
   if(first==null){first=hand;BaselineOne();}
   else if(second==null){second=hand;BaselineTwo();}
  }
  void BaselineOne(){offset=transform.position-first.Point;startRotation=transform.rotation;startHandRotation=first.Rotation;}
  void BaselineTwo(){startPosition=transform.position;startScale=transform.localScale;startRotation=transform.rotation;startMid=(first.Point+second.Point)*.5f;startAxis=second.Point-first.Point;startDistance=startAxis.magnitude;}
  public void Tick() {
   if(Locked){Cancel();return;}
   if(first==null)return;
   if(!first.Tracked||!first.GrabHeld){first=second;second=null;if(first!=null)BaselineOne();}
   if(first==null)return;
   if(second!=null&&(!second.Tracked||!second.GrabHeld)){second=null;BaselineOne();}
   if(second==null){var r=first.Rotation*Quaternion.Inverse(startHandRotation);transform.SetPositionAndRotation(first.Point+r*offset,r*startRotation);}
   else {
    Vector3 axis=second.Point-first.Point;
    if(startDistance<.025f||axis.magnitude<.025f)return;
    float scale=Mathf.Clamp(axis.magnitude/startDistance,.10f/startScale.x,2f/startScale.x);
    Quaternion turn=Quaternion.FromToRotation(startAxis,axis);
    transform.localScale=startScale*scale;
    transform.SetPositionAndRotation((first.Point+second.Point)*.5f+turn*(startPosition-startMid)*scale,turn*startRotation);
   }
  }
 }
}
