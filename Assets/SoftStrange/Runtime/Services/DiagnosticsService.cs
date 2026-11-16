using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
namespace SoftStrange {
 public sealed class DiagnosticsService : IDisposable {
  readonly Queue<string> events=new Queue<string>();
  StreamWriter writer;
  int frames;
  float accumulated;
  public float Fps {get;private set;}
  public string Path {get;private set;}
  public bool Recording=>writer!=null;
  public string Json {get;private set;}="{}";
  public string EventsJson=>"{\"events\":["+string.Join(",",events)+"]}";
  public DiagnosticsService(){Application.logMessageReceived+=OnLog;}
  public void Tick(float dt){frames++;accumulated+=dt;if(accumulated>=.5f){Fps=frames/accumulated;frames=0;accumulated=0;}}
  public void Record(bool enabled){
   writer?.Dispose();writer=null;if(!enabled)return;
   string directory=System.IO.Path.Combine(Application.persistentDataPath,"Diagnostics");Directory.CreateDirectory(directory);
   var files=new DirectoryInfo(directory).GetFiles("*.jsonl");Array.Sort(files,(a,b)=>a.CreationTimeUtc.CompareTo(b.CreationTimeUtc));for(int i=0;i<files.Length-6;i++)files[i].Delete();
   Path=System.IO.Path.Combine(directory,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".jsonl");writer=new StreamWriter(Path,true);
  }
  public void Publish(RuntimeState state){state.fps=Fps;state.frameMs=Fps>0?1000/Fps:0;state.allocatedMB=Profiler.GetTotalAllocatedMemoryLong()/1048576f;state.logPath=Path;Json=JsonUtility.ToJson(state);if(writer!=null){writer.WriteLine(Json);writer.Flush();if(writer.BaseStream.Length>16*1024*1024){writer.Dispose();writer=null;}}}
  void OnLog(string message,string stack,LogType type){
   if(type==LogType.Log&&!message.StartsWith("SoftStrange"))return;
   string escaped=message.Replace("\\","\\\\").Replace("\"","\\\"").Replace("\n","\\n").Replace("\r","");
   events.Enqueue("{\"time\":"+Time.unscaledTime.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"type\":\""+type+"\",\"message\":\""+escaped+"\"}");while(events.Count>100)events.Dequeue();
  }
  public void Dispose(){Application.logMessageReceived-=OnLog;writer?.Dispose();}
 }
}
