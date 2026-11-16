using UnityEngine;
using TMPro;
namespace SoftStrange {
 public sealed class PinchButton : MonoBehaviour {
  public UnityEngine.UI.Button Button;
  public TMP_Text Label;
  public CozyIcon Icon;
  Color normal;
  float hoverUntil,amount,pressedAt=-10;
  void Start(){normal=Button.image.color;}
  public void Hover(float strength){hoverUntil=Time.unscaledTime+.08f;amount=Mathf.Max(.15f,strength);}
  public void Press(){if(Button==null||!Button.interactable)return;pressedAt=Time.unscaledTime;Button.onClick.Invoke();}
  void Update(){if(Button==null)return;float hover=Time.unscaledTime<hoverUntil?amount:0;Button.image.color=Color.Lerp(normal,new Color(.54f,.65f,.50f,.95f),hover*.5f);float pulse=Mathf.Clamp01(1-(Time.unscaledTime-pressedAt)/.18f);if(Icon!=null)Icon.transform.localScale=Vector3.one*(1+pulse*.10f);}
 }
}
