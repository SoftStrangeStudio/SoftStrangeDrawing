using UnityEngine;
namespace SoftStrange {
 public sealed class ComfortAudioService : System.IDisposable {
  readonly AudioSource ambience,feedback;
  readonly GameObject root;
  readonly AudioClip tap,settle,open;
  public bool Muted {get;private set;}
  public float Volume {get;private set;}
  public ComfortAudioService(Camera camera){
   root=new GameObject("Drawing soundscape");root.transform.SetParent(camera.transform,false);
   if(camera.GetComponent<AudioListener>()==null)camera.gameObject.AddComponent<AudioListener>();
   ambience=root.AddComponent<AudioSource>();ambience.clip=Resources.Load<AudioClip>("Audio/quiet-tide");ambience.loop=true;ambience.spatialBlend=0;ambience.playOnAwake=false;
   feedback=root.AddComponent<AudioSource>();feedback.spatialBlend=.7f;feedback.rolloffMode=AudioRolloffMode.Linear;feedback.minDistance=.2f;feedback.maxDistance=3;feedback.playOnAwake=false;
   tap=Resources.Load<AudioClip>("Audio/pinch");settle=Resources.Load<AudioClip>("Audio/settle");open=Resources.Load<AudioClip>("Audio/open");
   Volume=PlayerPrefs.GetFloat("audio.volume",.5f);Muted=PlayerPrefs.GetInt("audio.muted",0)==1;Apply();if(ambience.clip!=null)ambience.Play();
  }
  public void SetVolume(float volume){Volume=Mathf.Clamp01(volume);PlayerPrefs.SetFloat("audio.volume",Volume);Apply();}
  public void SetMuted(bool muted){Muted=muted;PlayerPrefs.SetInt("audio.muted",muted?1:0);Apply();}
  void Apply(){ambience.volume=Muted?0:Volume*.10f;feedback.volume=Muted?0:Volume*.28f;}
  public void Cue(string action){if(Muted||AudioListener.pause)return;var clip=action=="lock"?settle:action=="image.select"||action=="draw"?open:tap;if(clip!=null)feedback.PlayOneShot(clip);}
  public void Dispose(){Object.Destroy(root);}
 }
}
