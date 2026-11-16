using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoftStrange {
 public interface IAppPlugin : IDisposable {
  string Id { get; }
  void Initialize(DrawingApp app, ServiceRegistry services);
  void Tick(float deltaTime);
 }
 public sealed class ServiceRegistry {
  readonly Dictionary<Type,object> services=new Dictionary<Type,object>();
  public void Register<T>(T service) where T:class {services[typeof(T)]=service;}
  public T Get<T>() where T:class {return services.TryGetValue(typeof(T),out var service)?(T)service:null;}
 }
 [Serializable] public sealed class AppCommand {
  public string id,name,text;
  public float value;
  public bool flag;
  public Vector3 vector;
 }
 [Serializable] public sealed class CommandDescription {
  public string name,description;
 }
 public sealed class CommandService {
  readonly Dictionary<string,Action<AppCommand>> handlers=new Dictionary<string,Action<AppCommand>>();
  public readonly List<CommandDescription> Descriptions=new List<CommandDescription>();
  public string LastId,LastName,LastError;
  public void Register(string name,string description,Action<AppCommand> handler){handlers.Add(name,handler);Descriptions.Add(new CommandDescription{name=name,description=description});}
  public void Execute(AppCommand command){
   LastId=command.id;LastName=command.name;LastError=null;
   if(string.IsNullOrEmpty(command.name)||!handlers.TryGetValue(command.name,out var handler))throw new ArgumentException("Unknown command: "+command.name);
   try{handler(command);Debug.Log("SoftStrange command: "+command.name);}catch(Exception e){LastError=e.Message;throw;}
  }
 }
 [Serializable] public sealed class RuntimeState {
  public string version,platform,inputMode,reference,lastCommand,lastCommandId,lastError,logPath,color;
  public string drawer,perspective;
  public bool inputAllowed,gridSelected,audioMuted;
  public float leftPinchStrength,rightPinchStrength;
  public Vector3 gridPosition,gridScale,menuPosition;
  public Quaternion gridRotation,menuRotation;
  public int frame,imageIndex,imageCount,uploadCount,curvePaths;
  public float seconds,fps,frameMs,allocatedMB,opacity,threshold,lineWidth,dotSpacing,dotSize;
  public bool home,dotted,curvesReady,curvesBusy,xrRunning,leftTracked,rightTracked,leftPinching,rightPinching,locked,visible,guides,debugPanel,simulation;
  public Vector3 headPosition,sheetPosition,sheetScale,leftPinchPosition,rightPinchPosition,leftRayDirection,rightRayDirection;
  public Quaternion sheetRotation;
 }
}
