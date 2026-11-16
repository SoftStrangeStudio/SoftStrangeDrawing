using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using TMPro;

namespace SoftStrange.Editor {
 public static class ProjectBuilder {
  const string ScenePath="Assets/SoftStrange/Scenes/Drawing.unity";
  public static void BuildDelivery(){BuildReleaseCandidate();Build();BuildMac();}
  [MenuItem("Soft Strange/Configure and build Quest")]
  public static void Build(){Configure();var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="builds/SoftStrangeDrawing.apk",target=BuildTarget.Android,options=BuildOptions.Development});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Android build failed: "+result.summary.result);Debug.Log("SoftStrange APK: "+result.summary.totalSize+" bytes");}
  [MenuItem("Soft Strange/Preview in Editor")]
  public static void Preview(){EditorSceneManager.OpenScene(ScenePath);EditorApplication.EnterPlaymode();}
  [MenuItem("Soft Strange/Configure project")]
  public static void Configure(){
   PlayerSettings.companyName="Soft Strange";PlayerSettings.productName="Soft Strange Drawing";
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.softstrange.drawing");PlayerSettings.bundleVersion="0.3.1";PlayerSettings.Android.bundleVersionCode=4;
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
   PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
   PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel32;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel34;
   PlayerSettings.Android.preferredInstallLocation=AndroidPreferredInstallLocation.Auto;
   PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan});
   var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/SoftStrange/Branding/AppIcon.png");if(icon!=null)PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
   PlayerSettings.SplashScreen.show=false;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.runInBackground=true;
   PlayerSettings.Android.forceInternetPermission=true;PlayerSettings.Android.useCustomKeystore=false;
   var player=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=player.FindProperty("activeInputHandler");if(input!=null){input.intValue=1;player.ApplyModifiedPropertiesWithoutUndo();}
   Directory.CreateDirectory("Assets/XR");
   var all=AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>("Assets/XR/GeneralSettings.asset");
   if(all==null){all=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(all,"Assets/XR/GeneralSettings.asset");EditorBuildSettings.AddConfigObject("com.unity.xr.management.loader_settings",all,true);}
   if(!all.HasSettingsForBuildTarget(BuildTargetGroup.Android))all.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
   var general=all.SettingsForBuildTarget(BuildTargetGroup.Android);general.InitManagerOnStart=true;
   if(general.Manager==null){var manager=ScriptableObject.CreateInstance<XRManagerSettings>();AssetDatabase.AddObjectToAsset(manager,all);general.Manager=manager;}
   if(!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Android))throw new Exception("Cannot assign OpenXR loader");
   UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
   var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);xr.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;xr.latencyOptimization=OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;
   foreach(var feature in xr.GetFeatures<OpenXRFeature>()){
    string name=feature.GetType().Name;
    feature.enabled=name=="MetaQuestFeature"||name=="MetaHandTrackingAim"||name=="HandTracking"||name=="ARCameraFeature"||name=="ARSessionFeature"||name=="OpenXRLifeCycleFeature"||name=="OpenXRCompositionLayersFeature"||name=="OculusTouchControllerProfile";
    if(feature.enabled)Debug.Log("Enabled XR feature: "+feature.GetType().FullName);
   }
   EditorUtility.SetDirty(xr);EditorUtility.SetDirty(all);EditorUtility.SetDirty(general);EditorUtility.SetDirty(general.Manager);
   SetupFont();
   foreach(string name in new[]{"Pointer","Sculpture"}){string path="Assets/SoftStrange/Resources/"+name+".mat";if(AssetDatabase.LoadAssetAtPath<Material>(path)==null){var mat=new Material(Shader.Find(name=="Pointer"?"SoftStrange/Pointer":"Standard"));mat.color=name=="Pointer"?new Color(.75f,.9f,.82f,.3f):new Color(.42f,.63f,.55f);if(name=="Sculpture")mat.SetFloat("_Glossiness",.25f);AssetDatabase.CreateAsset(mat,path);}}
   SetupReferencesAndHouse();
   Material sheet=AssetDatabase.LoadAssetAtPath<Material>("Assets/SoftStrange/Resources/Reference.mat");if(sheet==null){sheet=new Material(Shader.Find("SoftStrange/Reference"));AssetDatabase.CreateAsset(sheet,"Assets/SoftStrange/Resources/Reference.mat");}
   Material hand=AssetDatabase.LoadAssetAtPath<Material>("Assets/SoftStrange/Resources/Hands.mat");if(hand==null){hand=new Material(Shader.Find("Unlit/Color")){color=new Color(.3f,.9f,.8f)};AssetDatabase.CreateAsset(hand,"Assets/SoftStrange/Resources/Hands.mat");}
   if(!File.Exists(ScenePath)){
    var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    new GameObject("AR Session",typeof(ARSession));
    var originGo=new GameObject("XR Origin");var origin=originGo.AddComponent<XROrigin>();var offset=new GameObject("Camera offset");offset.transform.SetParent(originGo.transform,false);
    var cameraGo=new GameObject("Main Camera",typeof(Camera));cameraGo.tag="MainCamera";cameraGo.transform.SetParent(offset.transform,false);var camera=cameraGo.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.nearClipPlane=.02f;camera.farClipPlane=20;
    origin.Camera=camera;origin.CameraFloorOffsetObject=offset;origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Device;origin.CameraYOffset=0;
    cameraGo.AddComponent<ARCameraManager>();var tracking=cameraGo.AddComponent<TrackedPoseDriver>();
    tracking.positionInput=new InputActionProperty(new InputAction("Head position",InputActionType.Value,"<XRHMD>/centerEyePosition"));tracking.rotationInput=new InputActionProperty(new InputAction("Head rotation",InputActionType.Value,"<XRHMD>/centerEyeRotation"));tracking.trackingStateInput=new InputActionProperty(new InputAction("Head state",InputActionType.Value,"<XRHMD>/trackingState"));
    var app=new GameObject("Soft Strange Drawing").AddComponent<DrawingApp>();app.View=camera;app.Origin=offset.transform;app.SheetMaterial=sheet;app.HandMaterial=hand;app.Font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
    EditorSceneManager.SaveScene(scene,ScenePath);
   }
   var savedScene=EditorSceneManager.OpenScene(ScenePath);
   var savedApp=UnityEngine.Object.FindFirstObjectByType<DrawingApp>();
   if(UnityEngine.Object.FindFirstObjectByType<Light>()==null){var sun=new GameObject("Studio daylight").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.0f;sun.color=new Color(1,.94f,.84f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(42,-30,0);RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.45f,.50f,.55f);RenderSettings.ambientEquatorColor=new Color(.26f,.27f,.25f);RenderSettings.ambientGroundColor=new Color(.16f,.12f,.09f);EditorSceneManager.MarkSceneDirty(savedScene);EditorSceneManager.SaveScene(savedScene);}
   if(savedApp.Font==null){savedApp.Font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");if(savedApp.Font==null)throw new Exception("Missing TMP font; finish importing essentials before building");EditorSceneManager.MarkSceneDirty(savedScene);EditorSceneManager.SaveScene(savedScene);}
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};AssetDatabase.SaveAssets();
  }
  [MenuItem("Soft Strange/Build Quest release candidate")]
  public static void BuildReleaseCandidate(){Configure();PlayerSettings.Android.forceInternetPermission=false;try{var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="builds/SoftStrangeDrawing-ReleaseCandidate.apk",target=BuildTarget.Android,options=BuildOptions.None});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Release candidate build failed");}finally{PlayerSettings.Android.forceInternetPermission=true;}}
  [MenuItem("Soft Strange/Build Mac Studio")]
  public static void BuildMac(){BuildDesktop(BuildTarget.StandaloneOSX,"builds/Mac/Soft Strange Drawing.app",BuildOptions.Development);}
  [MenuItem("Soft Strange/Build Linux Studio")]
  public static void BuildLinux(){BuildDesktop(BuildTarget.StandaloneLinux64,"builds/Linux/SoftStrangeDrawing.x86_64",BuildOptions.None);}
  static void BuildDesktop(BuildTarget target,string path,BuildOptions options){
   Configure();PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=1000;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
   var scene=EditorSceneManager.OpenScene(ScenePath);foreach(var session in UnityEngine.Object.FindObjectsByType<ARSession>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(session);foreach(var camera in UnityEngine.Object.FindObjectsByType<ARCameraManager>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(camera);foreach(var tracking in UnityEngine.Object.FindObjectsByType<TrackedPoseDriver>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(tracking);foreach(var origin in UnityEngine.Object.FindObjectsByType<XROrigin>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(origin);EditorSceneManager.SaveScene(scene,"Assets/SoftStrange/Scenes/Studio.unity");
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/SoftStrange/Scenes/Studio.unity"},locationPathName=path,target=target,options=options});if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Desktop build failed: "+result.summary.result);
  }
  public static bool BuildingSteamFrame {get;private set;}
  [MenuItem("Soft Strange/Build Steam Frame Candidate")]
  public static void BuildSteamFrame(){
   Configure();BuildingSteamFrame=true;
   try{
    var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
    foreach(var feature in xr.GetFeatures<OpenXRFeature>()){string name=feature.GetType().Name;feature.enabled=name=="HandTracking"||name=="OculusTouchControllerProfile"||name=="SteamFrameControllerProfile"||name=="ValveOpenXRSupportFeature"||name=="ValveOpenXRLeptonValidationFeature"||name=="SteamFramePassthroughFeature"||name=="OpenXRCompositionLayersFeature";if(feature.enabled)Debug.Log("Steam Frame feature: "+name);}
    PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
    EditorUtility.SetDirty(xr);AssetDatabase.SaveAssets();
    var scene=EditorSceneManager.OpenScene(ScenePath);
    foreach(var session in UnityEngine.Object.FindObjectsByType<ARSession>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(session);
    foreach(var camera in UnityEngine.Object.FindObjectsByType<ARCameraManager>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(camera);
    var app=UnityEngine.Object.FindFirstObjectByType<DrawingApp>();app.PreferControllers=true;app.View.clearFlags=CameraClearFlags.SolidColor;app.View.backgroundColor=Color.clear;
    const string frameScene="Assets/SoftStrange/Scenes/SteamFrame.unity";EditorSceneManager.SaveScene(scene,frameScene);
    var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{frameScene},locationPathName="builds/SoftStrangeDrawing-SteamFrame.apk",target=BuildTarget.Android,options=BuildOptions.Development});
    if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Steam Frame build failed");
   }finally{BuildingSteamFrame=false;Configure();}
  }

  static void SetupReferencesAndHouse(){
   foreach(string file in Directory.GetFiles("Assets/SoftStrange/Resources","*.png",SearchOption.AllDirectories)){var importer=AssetImporter.GetAtPath(file) as TextureImporter;if(importer!=null&&(!importer.isReadable||importer.maxTextureSize!=(file.Contains("PairedReferences")?1024:2048)||importer.npotScale!=TextureImporterNPOTScale.None)){importer.isReadable=true;importer.npotScale=TextureImporterNPOTScale.None;importer.alphaIsTransparency=true;importer.maxTextureSize=file.Contains("PairedReferences")?1024:2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}}
   if(AssetDatabase.LoadAssetAtPath<Material>("Assets/SoftStrange/Resources/Curves.mat")==null){var curves=new Material(Shader.Find("SoftStrange/CurveStroke"));AssetDatabase.CreateAsset(curves,"Assets/SoftStrange/Resources/Curves.mat");}
   var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/SoftStrange/Environment/DrawingHouse.fbx");if(model!=null){var instance=UnityEngine.Object.Instantiate(model);instance.name="Drawing House";foreach(var renderer in instance.GetComponentsInChildren<Renderer>()){foreach(var material in renderer.sharedMaterials){if(material!=null&&material.name.Contains("glass")){material.SetFloat("_Mode",3);material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);material.SetInt("_ZWrite",0);material.EnableKeyword("_ALPHABLEND_ON");material.renderQueue=3000;}}}PrefabUtility.SaveAsPrefabAsset(instance,"Assets/SoftStrange/Resources/DrawingHouse.prefab");UnityEngine.Object.DestroyImmediate(instance);}
  }
  static void SetupFont(){
   if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset")!=null)return;
   TMP_PackageResourceImporter.ImportResources(true,false,false);
   AssetDatabase.Refresh();
   if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset")==null)
    throw new Exception("TMP essentials import is pending. Wait for import to finish, then build again.");
  }
 }
}
