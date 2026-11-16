using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace SoftStrange {
 public sealed class ImageLibraryService : IDisposable {
  sealed class Entry {public string Id,Name,Path,Side,Top,Model;public bool AI,Shark;}
  readonly List<Entry> entries=new List<Entry>();
  readonly Dictionary<string,Texture2D> thumbnails=new Dictionary<string,Texture2D>();
  readonly string folder,inbox;
  Texture2D loaded;
  public Texture2D Texture {get;private set;}
  public int Index {get;private set;}
  public string Perspective {get;private set;}="side";
  public int Count=>entries.Count;
  public int UploadCount {get;private set;}
  public bool IsBundled=>entries.Count>0&&entries[Index].Side!=null;
  public bool HasPair=>entries.Count>0&&entries[Index].Top!=null;
  public string ModelPath=>entries.Count>0?entries[Index].Model:null;
  public string Name=>entries.Count>0?entries[Index].Name:"Your outline";
  public string Id=>entries.Count>0?entries[Index].Id:"";
  public bool IsLineArt {get;private set;}
  public event Action Changed;
  public ImageLibraryService(){
   folder=Path.Combine(Application.persistentDataPath,"Images");inbox=Path.Combine(Application.persistentDataPath,"Inbox");Directory.CreateDirectory(folder);Directory.CreateDirectory(inbox);
   var paired=Resources.Load<TextAsset>("PairedCatalog");
   if(paired!=null)foreach(string row in paired.text.Split('\n')){var c=row.Trim().Split('|');if(c.Length==5)entries.Add(new Entry{Id=c[0],Name=c[1],Side=c[2],Top=c[3],Model=c[4],Shark=int.TryParse(c[0],out int number)&&number<=50});}
   var catalog=Resources.Load<TextAsset>("ReferenceCatalog");if(catalog!=null)foreach(string row in catalog.text.Split('\n')){var c=row.Trim().Split('|');if(c.Length==2){bool shark=int.TryParse(c[0].Split('-')[0],out int number)&&number<=50;entries.Add(new Entry{Id=c[0],Name=c[1],Side="References/"+c[0],AI=true,Shark=shark,Model="ReferenceModels/"+(shark?"01":number.ToString("D2"))+"-model"});}}
   var paths=Directory.GetFiles(folder,"*.image");Array.Sort(paths,StringComparer.Ordinal);
   foreach(string path in paths)entries.Add(new Entry{Id=Path.GetFileName(path),Name="My reference "+(++UploadCount),Path=path});
   int selected=entries.FindIndex(e=>e.Id==PlayerPrefs.GetString("reference.id","28-study"));try{Select(Mathf.Max(0,selected),PlayerPrefs.GetString("reference.view","side"));}catch(Exception e){Debug.LogWarning("Saved reference unavailable: "+e.Message);Select(0,"side");}
  }
  public string NameAt(int index)=>entries[index].Name;
  public bool HasPairAt(int index)=>entries[index].Top!=null;
  public bool IsAIAt(int index)=>entries[index].AI;
  public bool IsImportedAt(int index)=>entries[index].Path!=null;
  public bool IsSharkAt(int index)=>entries[index].Shark;
  public Texture2D GetThumbnail(int index,string view="side"){
   var e=entries[index];string path=e.Path??(view=="top"&&e.Top!=null?e.Top:e.Side);
   if(thumbnails.TryGetValue(path,out var existing))return existing;
   try{Texture2D image=e.Path==null?Resources.Load<Texture2D>(path):Thumbnail(Decode(File.ReadAllBytes(e.Path)));thumbnails[path]=image;return image;}catch(Exception error){Debug.LogWarning("Reference thumbnail unavailable: "+error.Message);return null;}
  }
  static Texture2D Thumbnail(Texture2D source){
   if(!LooksLikeTransparentLines(source)||Mathf.Max(source.width,source.height)<=128)return source;
   float scale=128f/Mathf.Max(source.width,source.height);int w=Mathf.Max(1,Mathf.RoundToInt(source.width*scale)),h=Mathf.Max(1,Mathf.RoundToInt(source.height*scale));var pixels=source.GetPixels32();var output=new Color32[w*h];
   // Preserve fine strokes at gallery size by retaining maximum alpha per source cell.
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){Color32 strongest=default;for(int yy=y*source.height/h;yy<(y+1)*source.height/h;yy++)for(int xx=x*source.width/w;xx<(x+1)*source.width/w;xx++){var c=pixels[yy*source.width+xx];if(c.a>strongest.a)strongest=c;}output[y*w+x]=strongest;}
   var thumbnail=new Texture2D(w,h,TextureFormat.RGBA32,false);thumbnail.SetPixels32(output);thumbnail.Apply();UnityEngine.Object.Destroy(source);return thumbnail;
  }
  public void ReleaseThumbnails(){foreach(var item in thumbnails){if(item.Value==Texture)continue;if(item.Key.EndsWith(".image"))UnityEngine.Object.Destroy(item.Value);else if(item.Value!=null)Resources.UnloadAsset(item.Value);}thumbnails.Clear();}
  static Texture2D Decode(byte[] bytes){if(bytes.Length>UploadServer.MaxBytes||!UploadServer.ImageSizeAllowed(bytes))throw new IOException("Use PNG/JPEG up to 4096 pixels and 12 MB.");var image=new Texture2D(2,2,TextureFormat.RGBA32,true);if(!image.LoadImage(bytes,false)){UnityEngine.Object.Destroy(image);throw new IOException("Could not read this image.");}return image;}
  public void Select(int index,string view=null){
   if(entries.Count==0)throw new IOException("The offline reference library is missing.");
   int next=((index%entries.Count)+entries.Count)%entries.Count;var e=entries[next];string perspective=view??Perspective;perspective=perspective=="top"&&e.Top!=null?"top":"side";
   Texture2D image=e.Path!=null?Decode(File.ReadAllBytes(e.Path)):Resources.Load<Texture2D>(perspective=="top"?e.Top:e.Side);
   if(image==null)throw new IOException("Missing reference "+e.Name);
   var previous=Texture;if(loaded!=null){UnityEngine.Object.Destroy(loaded);loaded=null;}
   if(previous!=null&&IsBundled&&previous!=image&&!thumbnails.ContainsValue(previous))Resources.UnloadAsset(previous);
   Index=next;Perspective=perspective;Texture=image;image.wrapMode=TextureWrapMode.Clamp;if(e.Path!=null)loaded=image;
   IsLineArt=IsBundled||LooksLikeTransparentLines(image);
   PlayerPrefs.SetString("reference.id",e.Id);PlayerPrefs.SetString("reference.view",Perspective);Changed?.Invoke();
  }
  static bool LooksLikeTransparentLines(Texture2D image){int clear=0,opaque=0;for(int y=0;y<16;y++)for(int x=0;x<16;x++){var c=image.GetPixel(x*image.width/16,y*image.height/16);if(c.a<.1f)clear++;if(c.a>.7f)opaque++;}return clear>190&&opaque<65;}
  public void Import(byte[] bytes){
   if(UploadCount>=100)throw new IOException("Your library has reached 100 imported images.");var check=Decode(bytes);UnityEngine.Object.Destroy(check);
   string path=Path.Combine(folder,DateTime.UtcNow.Ticks+".image");string staging=path+".tmp";File.WriteAllBytes(staging,bytes);File.Move(staging,path);entries.Add(new Entry{Id=Path.GetFileName(path),Name="My reference "+(++UploadCount),Path=path});Select(entries.Count-1,"side");Debug.Log("SoftStrange image imported");
  }
  public void ScanInbox(){foreach(string path in Directory.GetFiles(inbox,"*.image")){try{if(new FileInfo(path).Length>UploadServer.MaxBytes)throw new IOException("Image exceeds 12 MB");Import(File.ReadAllBytes(path));File.Move(path,path+".imported");}catch(Exception e){Debug.LogWarning("Image import: "+e.Message);File.Move(path,path+".rejected");}}}
  public void Dispose(){ReleaseThumbnails();if(loaded!=null)UnityEngine.Object.Destroy(loaded);else if(Texture!=null)Resources.UnloadAsset(Texture);}
 }
}
