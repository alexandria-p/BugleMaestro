using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BugleMaestro.Patches;
using BugleMaestro.Helpers;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

namespace BugleMaestro;

// This BepInAutoPlugin attribute comes from the Hamunii.BepInEx.AutoPlugin NuGet package
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;

    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony? _harmonyInstance;
    public readonly static string AUTHOR_NAME = "alexandria-p";
    public readonly static string MOD_NAME = "BugleMaestro";
    public readonly static string LOG_PREFIX = MOD_NAME;
    public readonly static string DEFAULT_CHARACTER_NAME = "DEFAULT_NAME";

    // Audio
    public Dictionary<ScaleEnum, AudioClip> baseBugleClips = new Dictionary<ScaleEnum, AudioClip>();

    // UI display
    public GUIManager guiManager;
    public TextMeshProUGUI itemInfoDisplayTextMesh;
    public Dictionary<string, string> fontColors = new Dictionary<string, string>();
    // UI display configuration
    public float fontSize;
    public float outlineWidth;
    public float lineSpacing;
    public float sizeDeltaX;

    private void Awake()
    {
        Instance = this;
        Log = Logger;

        // UI setup
        UIHelper.SetupUIElements();
        ClipHelper.SetupBaseBugleClips();

        // Patch
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(CharacterMovementPatch));
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(BugleSFXPatch));
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(CharacterItemsPatch));
        Log.LogInfo($"{LOG_PREFIX}: Plugin {Name} is loaded!");
    }

    private void OnDestroy()
    {
        Log.LogInfo($"{LOG_PREFIX}: Plugin destroying...");
        Log.LogDebug($"{LOG_PREFIX}: Removing harmony patches...");
        // todo - check this
        if ( _harmonyInstance != null )
        {
            _harmonyInstance!.UnpatchSelf();
        }
        else
        {
            Log.LogInfo($"{LOG_PREFIX}: null harmony instance OnDestroy");
        }
        Log.LogInfo($"{LOG_PREFIX}: Plugin destroyed!");
    }
}
