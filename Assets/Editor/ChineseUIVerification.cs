using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class CompleteDemoVerification
{
    private static void VerifyChinesePresentation()
    {
        var ui=Object.FindObjectOfType<GameUI>();
        var state=Object.FindObjectOfType<GameFlow>().State;
        var data=new SerializedObject(ui);
        var fill=(UnityEngine.UI.Image)data.FindProperty("healthFill").objectReferenceValue;
        foreach(var image in ui.GetComponentsInChildren<UnityEngine.UI.Image>(true))
        {
            bool graphic=image==fill || image.sprite!=null && (image.name=="MenuArtwork" || image.name=="Icon" || new[]{"Heal","Barrage","Giant","Shield"}.Contains(image.name));
            bool dialogImage=image.transform.IsChildOf(ui.transform.Find("Modal/Dialog"));
            Require(graphic || image.color.a<=(dialogImage?0.9f:0.6f),"UI background is too opaque: "+image.name);
            if(new[]{"HUD","MainMenu","Modal"}.Contains(image.name)) Require(image.color.a==0,"Full-screen UI cover obscures world: "+image.name);
            if(new[]{"HealthPanel","WavePanel","RunPanel","BarrageCard","GiantCard","ShieldCard"}.Contains(image.name)) Require(image.color.a>=0.25f && image.color.a<=0.45f,"Missing or excessive compact backdrop: "+image.name);
            if(graphic) Require(image.color.a>0,"UI icon/health graphic was hidden: "+image.name);
        }
        var rects=ui.GetComponentsInChildren<RectTransform>(true);
        var artwork=rects.First(r=>r.name=="MenuArtwork");
        Require(!artwork.GetComponent<UnityEngine.UI.Image>().raycastTarget,"Menu artwork intercepts clicks");
        Require(artwork.GetSiblingIndex()==0 && artwork.GetComponent<UnityEngine.UI.Image>().sprite!=null,"Menu artwork is not behind existing controls");
        Require(artwork.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectMode==UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent,"Menu artwork must fill without stretching");
        var controls=rects.First(r=>r.name=="Controls");
        Require(controls.anchoredPosition==new Vector2(0,18) && controls.sizeDelta==new Vector2(820,30),"Accepted controls layout changed");
        Require(controls.GetComponentsInChildren<ArenaPanelFrame>(true).Length==0,"Controls received unwanted trim");
        foreach(var frame in ui.GetComponentsInChildren<ArenaPanelFrame>(true))
        {
            Require(!frame.raycastTarget,"Decoration intercepts input");
            Require(frame.GetComponent<CanvasRenderer>()!=null,"Trim is missing its renderer");
        }
        foreach(var card in rects.Where(r=>new[]{"BarrageCard","GiantCard","ShieldCard"}.Contains(r.name)))
        {
            Require(card.anchoredPosition.y<85 && card.anchoredPosition.y>65,"Item UI was not lowered: "+card.name);
            Require(card.anchoredPosition.y-card.sizeDelta.y/2>controls.anchoredPosition.y+controls.sizeDelta.y/2+4,"Item UI overlaps bottom controls");
        }
        var wave=rects.First(r=>r.name=="WavePanel");
        Require(wave.sizeDelta.x<=340 && wave.sizeDelta.y<=75,"Wave backdrop takes excessive screen space");
        if(state==GameFlow.ScreenState.Paused || state==GameFlow.ScreenState.Victory || state==GameFlow.ScreenState.Defeat)
        {
            var dialog=(RectTransform)ui.transform.Find("Modal/Dialog");
            Require(dialog.sizeDelta.x<=500 && dialog.sizeDelta.y<=440,"Modal remains oversized");
            Require(dialog.Find("StatusEmblem").GetComponent<ArenaStatusEmblem>()!=null,"Modal lacks status emblem");
            Require(!dialog.Find("StatusEmblem").GetComponent<ArenaStatusEmblem>().raycastTarget,"Emblem intercepts clicks");
            var summary=dialog.Find("ModalStats/ModalSummary").GetComponent<TMP_Text>();
            Require(summary.text.Contains("击败") && summary.text.Contains("用时"),"Missing modal statistics");
            if(state!=GameFlow.ScreenState.Paused)
                Require(dialog.Find("RestartButton").GetComponent<RectTransform>().anchoredPosition.y>-120,"Results retain empty resume row");
        }
        var texts=ui.GetComponentsInChildren<TMP_Text>(true);
        foreach(var text in texts)
        {
            foreach(char c in text.text.Where(c=>!char.IsWhiteSpace(c))) Require(text.font.HasCharacter(c),"Missing font glyph: "+text.name+" / "+c);
            foreach(Match word in Regex.Matches(text.text,"[A-Za-z]{2,}")) Require(new[]{"WASD","Shift","Esc"}.Contains(word.Value),"UI English text remains: "+text.text);
        }
        int pauseCount=texts.Count(t=>t.gameObject.activeInHierarchy && t.text.Contains("暂停"));
        int expected=state==GameFlow.ScreenState.Playing || state==GameFlow.ScreenState.Paused ? 1:0;
        Require(pauseCount==expected,"Duplicate/missing pause prompt: "+pauseCount+" in "+state);
    }
}
