using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif
namespace SoftStrange {
#if UNITY_EDITOR
 [OpenXRFeature(UiName="Soft Strange Steam Frame passthrough",Company="Soft Strange",Version="1.0.0",BuildTargetGroups=new[]{BuildTargetGroup.Android},FeatureId="com.softstrange.steamframe.passthrough")]
#endif
 public sealed class SteamFramePassthroughFeature : OpenXRFeature {
  protected override void OnSessionCreate(ulong session){
   // Core OpenXR composition, independent of Meta's AR camera/passthrough APIs.
   SetEnvironmentBlendMode(XrEnvironmentBlendMode.AlphaBlend);
   Debug.Log("SoftStrange Steam Frame environment blend mode: "+GetEnvironmentBlendMode());
  }
 }
}
