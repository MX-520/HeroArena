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
            bool graphic=image==fill || image.sprite!=null && (image.name=="Icon" || new[]{"Heal","Barrage","Giant","Shield"}.Contains(image.name));
            Require(graphic || image.color.a==0,"UI background is not completely transparent: "+image.name);
            if(graphic) Require(image.color.a>0,"UI icon/health graphic was hidden: "+image.name);
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
