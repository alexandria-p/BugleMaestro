using BepInEx;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BugleMaestro.Helpers;

// From https://thunderstore.io/c/peak/p/jkqt/ItemInfoDisplay/
public class UIHelper
{
    public readonly static float DEFAULT_FONTSIZE = 20f;
    public readonly static float DEFAULT_OUTLINEWIDTH = 0.08f;
    public readonly static float DEFAULT_LINESPACING = -35f;
    public readonly static float DEFAULT_SIZEDELTAX = 550f;

    public static void AddDisplayObject()
    {
        GameObject guiManagerGameObj = GameObject.Find("GAME/GUIManager");
        Plugin.Instance.guiManager = guiManagerGameObj.GetComponent<GUIManager>();
        TMPro.TMP_FontAsset font = Plugin.Instance.guiManager.heroDayText.font;

        GameObject invGameObj = guiManagerGameObj.transform.Find("Canvas_HUD/Prompts/ItemPromptLayout").gameObject;
        GameObject itemInfoDisplayGameObj = new GameObject("BugleMaestroDisplay");
        itemInfoDisplayGameObj.transform.SetParent(invGameObj.transform);
        Plugin.Instance.itemInfoDisplayTextMesh = itemInfoDisplayGameObj.AddComponent<TextMeshProUGUI>();
        RectTransform itemInfoDisplayRect = itemInfoDisplayGameObj.GetComponent<RectTransform>();

        itemInfoDisplayRect.sizeDelta = new Vector2(Plugin.Instance.sizeDeltaX, 0f); // Y is 0, otherwise moves other item prompts     // configSizeDeltaX.Value
        Plugin.Instance.itemInfoDisplayTextMesh.font = font;
        Plugin.Instance.itemInfoDisplayTextMesh.fontSize = Plugin.Instance.fontSize; // configFontSize.Value
        Plugin.Instance.itemInfoDisplayTextMesh.alignment = TextAlignmentOptions.BottomLeft;
        Plugin.Instance.itemInfoDisplayTextMesh.lineSpacing = Plugin.Instance.lineSpacing; // configLineSpacing.Value
        Plugin.Instance.itemInfoDisplayTextMesh.text = "";
        Plugin.Instance.itemInfoDisplayTextMesh.outlineWidth = Plugin.Instance.outlineWidth; // configOutlineWidth.Value
    }

    public static void SetupUIElements()
    {
        InitEffectColors(Plugin.Instance.fontColors); // fills the dictionary with items
        Plugin.Instance.fontSize = DEFAULT_FONTSIZE;
        Plugin.Instance.outlineWidth = DEFAULT_OUTLINEWIDTH;
        Plugin.Instance.lineSpacing = DEFAULT_LINESPACING;
        Plugin.Instance.sizeDeltaX = DEFAULT_SIZEDELTAX;
       
        /*
        Plugin.Instance.configFontSize = ((BaseUnityPlugin)Plugin.Instance).Config.Bind<float>("BugleMaestroDisplay", "Font Size", 20f, "Customize the Font Size for description text.");
        Plugin.Instance.configOutlineWidth = ((BaseUnityPlugin)Plugin.Instance).Config.Bind<float>("BugleMaestroDisplay", "Outline Width", 0.08f, "Customize the Outline Width for item description text.");
        Plugin.Instance.configLineSpacing = ((BaseUnityPlugin)Plugin.Instance).Config.Bind<float>("BugleMaestroDisplay", "Line Spacing", -35f, "Customize the Line Spacing for item description text.");
        Plugin.Instance.configSizeDeltaX = ((BaseUnityPlugin)Plugin.Instance).Config.Bind<float>("BugleMaestroDisplay", "Size Delta X", 550f, "Customize the horizontal length of the container for the mod. Increasing moves text left, decreasing moves text right.");
        */
    }

    public static void InitEffectColors(Dictionary<string, string> dict)
    {
        dict.Add("Note", "<#A65A1C>");
    }

    public static void DisplayBugleNote(ScaleEnum note)
    {
        string noteDisplayName = ScaleHelper.GetAttributeOfScaleNote<UIDisplayNameAttribute>(note).UIDisplayName;
        string uiMessage = $"Currently playing: {noteDisplayName}";
        DisplayMessage(uiMessage, "Note");
    }

    public static void HideUI()
    {
        if (Plugin.Instance.guiManager == null)
        {
            AddDisplayObject();
        }

        if (Plugin.Instance.itemInfoDisplayTextMesh.gameObject.activeSelf)
        {
            Plugin.Instance.itemInfoDisplayTextMesh.gameObject.SetActive(false);
        }
    }

    public static void DisplayMessage(string message, string colourKey)
    {
        if (Plugin.Instance.guiManager == null)
        {
            AddDisplayObject();
        }

        if (!Plugin.Instance.itemInfoDisplayTextMesh.gameObject.activeSelf)
        {
            Plugin.Instance.itemInfoDisplayTextMesh.gameObject.SetActive(true);
        }
        Plugin.Instance.itemInfoDisplayTextMesh.text = "";
        Plugin.Instance.itemInfoDisplayTextMesh.text += $"{Plugin.Instance.fontColors[colourKey]} {message}</color>\n";
        Plugin.Instance.itemInfoDisplayTextMesh.text = Plugin.Instance.itemInfoDisplayTextMesh.text.Replace("\n\n\n", "\n\n");
    }
}