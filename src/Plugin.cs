using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BugleMaestro.Patches;
using BugleMaestro.Helpers;
using System.Collections.Generic;

namespace BugleMaestro;

//#if (!no-tutorial)
// Here are some basic resources on code style and naming conventions to help
// you in your first CSharp plugin!
// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
// https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
// https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces

// This BepInAutoPlugin attribute comes from the Hamunii.BepInEx.AutoPlugin
// NuGet package, and it will generate the BepInPlugin attribute for you!
// For more info, see https://github.com/Hamunii/BepInEx.AutoPlugin
//#endif
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;

    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony? _harmonyInstance;
    public readonly static string LOG_PREFIX = "BugleMaestro";

    // todo - track per BUGLE !?
    public ScaleEnum CurrentNote { get; set; } = ScaleHelper.DEFAULT_NOTE;
    public RawNoteInputEnum CurrentRawNote { get; set; } = ScaleHelper.DEFAULT_RAW_NOTE;
    public OctaveEnum CurrentOctave { get; set; } = ScaleHelper.DEFAULT_OCTAVE;
    public SemitoneModifierEnum CurrentSemitoneModifier { get; set; } = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    public bool IsANotePlaying { get; set; } = false;

    // Track inputs
    public HashSet<OctaveEnum> LastFrameOctaveInput { get; set; } = new HashSet<OctaveEnum>();
    public HashSet<SemitoneModifierEnum> LastFrameSemitoneInput { get; set; } = new HashSet<SemitoneModifierEnum>();
    public List<RawNoteInputEnum> LastFrameRawNoteInput { get; set; } = new List<RawNoteInputEnum>(); // list to preserve insertion order

    private void Awake()
    {
        Instance = this;
        Log = Logger;


        //_harmonyInstance = new Harmony(Info.Metadata.GUID).PatchAll(); // Info.Metadata.GUID // "com.github.PEAKModding.AlexModTest"
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(BugleSFXPatch)); // works
        Log.LogInfo($"{LOG_PREFIX}: Plugin {Name} is loaded!");
       
    }


    private void OnDestroy()
    {
        Log.LogInfo("{LOG_PREFIX}: Plugin destroying...");
        Log.LogDebug("{LOG_PREFIX}: Removing harmony patches...");
        // todo
        if ( _harmonyInstance != null )
        {
            _harmonyInstance!.UnpatchSelf();
        }
        else
        {
            Log.LogInfo("{LOG_PREFIX}: null harmony instance OnDestroy");
        }
        Log.LogInfo("{LOG_PREFIX}: Plugin destroyed!");
    }
}
