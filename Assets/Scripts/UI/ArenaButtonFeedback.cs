using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Explicit multi-graphic feedback, animated with unscaled time so paused menus still respond.</summary>
[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class ArenaButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Color restingFill;
    [SerializeField] private Color restingText;
    private UnityEngine.UI.Button button;
    private UnityEngine.UI.Image fill;
    private TMP_Text label;
    private ArenaPanelFrame trim;
    private bool hovered, selected, pressed;
    private float emphasis;

    private void Cache()
    {
        if(button!=null) return;
        button=GetComponent<UnityEngine.UI.Button>();
        fill=GetComponent<UnityEngine.UI.Image>();
        label=GetComponentInChildren<TMP_Text>(true);
        trim=GetComponentInChildren<ArenaPanelFrame>(true);
    }
    public void CaptureRestStyle()
    {
        Cache();restingFill=fill.color;restingText=label.color;
        button.transition=UnityEngine.UI.Selectable.Transition.None;
        button.targetGraphic=fill;
    }
    private void OnEnable() { Cache(); hovered=selected=pressed=false; emphasis=0; Paint(); }
    private void OnDisable()
    {
        hovered=selected=pressed=false;emphasis=0;
        if(fill!=null) Paint();
    }
    private void LateUpdate()
    {
        Cache();
        float target=button.IsInteractable() && (hovered||selected)?1:0;
        emphasis=Mathf.MoveTowards(emphasis,target,Time.unscaledDeltaTime*10);
        Paint();
    }
    private void Paint()
    {
        bool modal=transform.parent.name=="Dialog";
        Color highlight=pressed?new Color(0.24f,0.14f,0.035f,modal?0.88f:0.58f):new Color(0.46f,0.30f,0.075f,modal?0.88f:0.58f);
        fill.color=Color.Lerp(restingFill,highlight,emphasis);
        label.color=Color.Lerp(restingText,new Color(1,0.98f,0.75f),emphasis);
        if(trim!=null) trim.Configure(Color.Lerp(new Color(0.87f,0.77f,0.50f,0.65f),new Color(1,0.91f,0.45f,1),emphasis),8+emphasis*6);
        transform.localScale=Vector3.one*(1+emphasis*(pressed?0.004f:0.025f));
    }
    public void OnPointerEnter(PointerEventData data) { hovered=true; }
    public void OnPointerExit(PointerEventData data) { hovered=false;pressed=false; }
    public void OnPointerDown(PointerEventData data) { if(data.button==PointerEventData.InputButton.Left) pressed=true; }
    public void OnPointerUp(PointerEventData data) { pressed=false; }
    public void OnSelect(BaseEventData data) { selected=true; }
    public void OnDeselect(BaseEventData data) { selected=false;pressed=false; }
}
