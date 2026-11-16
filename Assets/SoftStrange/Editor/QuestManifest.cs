using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
namespace SoftStrange.Editor {
 // Passthrough composition needs no raw camera access. The Meta AR package adds
 // raw-camera permissions for its optional CPU-image API, which this app never uses.
 public sealed class QuestManifest : IPostGenerateGradleAndroidProject, IPreprocessBuildWithReport {
  static bool release;
  static bool steamFrame;
  public void OnPreprocessBuild(BuildReport report){release=(report.summary.options&BuildOptions.Development)==0;steamFrame=report.summary.outputPath.EndsWith("-SteamFrame.apk");}
  public int callbackOrder=>10000;
  public void OnPostGenerateGradleAndroidProject(string path){
   string file=Path.Combine(path,"src/main/AndroidManifest.xml");var doc=new XmlDocument();doc.Load(file);const string android="http://schemas.android.com/apk/res/android";
   foreach(XmlNode node in doc.SelectNodes("/manifest/uses-permission")){string name=node.Attributes["name",android]?.Value;if(name=="android.permission.CAMERA"||name=="horizonos.permission.HEADSET_CAMERA"||name=="com.oculus.permission.USE_PASSTHROUGH_CAMERA"||(release&&name=="android.permission.INTERNET"))node.ParentNode.RemoveChild(node);}
   foreach(string permission in new[]{"android.permission.CAMERA","horizonos.permission.HEADSET_CAMERA","com.oculus.permission.USE_PASSTHROUGH_CAMERA",release?"android.permission.INTERNET":null}){if(permission==null)continue;var node=doc.CreateElement("uses-permission");node.SetAttribute("name",android,permission);node.SetAttribute("node","http://schemas.android.com/tools","remove");doc.DocumentElement.AppendChild(node);}
   var app=(XmlElement)doc.SelectSingleNode("/manifest/application");app.SetAttribute("allowBackup",android,"false");
   if(steamFrame){
    foreach(XmlElement node in doc.SelectNodes("/manifest/uses-permission"))if(node.GetAttribute("name",android).StartsWith("com.oculus.")&&node.GetAttribute("node","http://schemas.android.com/tools")!="remove")node.ParentNode.RemoveChild(node);
    var handPermission=doc.CreateElement("uses-permission");handPermission.SetAttribute("name",android,"com.oculus.permission.HAND_TRACKING");handPermission.SetAttribute("node","http://schemas.android.com/tools","remove");doc.DocumentElement.AppendChild(handPermission);
    foreach(string feature in new[]{"oculus.software.handtracking","com.oculus.feature.PASSTHROUGH"}){var remove=doc.CreateElement("uses-feature");remove.SetAttribute("name",android,feature);remove.SetAttribute("node","http://schemas.android.com/tools","remove");doc.DocumentElement.AppendChild(remove);}
    foreach(XmlElement node in app.SelectNodes("meta-data"))if(node.GetAttribute("name",android).StartsWith("com.oculus."))node.ParentNode.RemoveChild(node);
    foreach(XmlElement node in app.SelectNodes("activity/intent-filter/category"))if(node.GetAttribute("name",android).StartsWith("com.oculus."))node.ParentNode.RemoveChild(node);
    doc.Save(file);return;
   }
   doc.DocumentElement.SetAttribute("installLocation",android,"auto");
   foreach(XmlElement activity in app.SelectNodes("activity"))if(activity.SelectSingleNode("intent-filter/action[@android:name='android.intent.action.MAIN']",Namespaces(doc,android))!=null)activity.SetAttribute("excludeFromRecents",android,"true");
   XmlElement splash=null;foreach(XmlElement node in app.SelectNodes("meta-data"))if(node.GetAttribute("name",android)=="com.oculus.ossplash.background")splash=node;
   if(splash==null){splash=doc.CreateElement("meta-data");app.AppendChild(splash);}splash.SetAttribute("name",android,"com.oculus.ossplash.background");splash.SetAttribute("value",android,"passthrough-contextual");doc.Save(file);
  }
  static XmlNamespaceManager Namespaces(XmlDocument doc,string android){var namespaces=new XmlNamespaceManager(doc.NameTable);namespaces.AddNamespace("android",android);return namespaces;}
 }
}
