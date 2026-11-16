using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
namespace SoftStrange.Editor {
 // XR package preprocessors preload target settings even when no desktop XR loader exists.
 // Leave Editor configuration intact, but omit those assets from non-XR desktop players.
 public sealed class DesktopBuildIsolation : IPreprocessBuildWithReport {
  public int callbackOrder=>10000;
  public void OnPreprocessBuild(BuildReport report){
   if(report.summary.platform!=BuildTarget.StandaloneOSX&&report.summary.platform!=BuildTarget.StandaloneLinux64)return;
   PlayerSettings.SetPreloadedAssets(PlayerSettings.GetPreloadedAssets().Where(asset=>!(asset is OpenXRSettings)&&!(asset is XRGeneralSettings)&&asset!=null&&!asset.GetType().Name.StartsWith("XRSimulation")).ToArray());
  }
 }
}
