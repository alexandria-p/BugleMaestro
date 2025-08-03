using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BugleMaestro.Patches;
using BugleMaestro.Helpers;

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

    // todo - track per player?
    public ScaleEnum CurrentNote { get; set; } = ScaleHelper.DEFAULT_NOTE;
    public OctaveEnum CurrentOctave { get; set; } = ScaleHelper.DEFAULT_OCTAVE;
    public SemitoneModifierEnum CurrentSemitoneModifier { get; set; } = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    public bool IsNotePlaying { get; set; } = false;

    private void Awake()
    {
        Instance = this;
        //#if (!no-tutorial)
        // BepInEx gives us a logger which we can use to log information.
        // See https://lethal.wiki/dev/fundamentals/logging
        //#endif
        Log = Logger;

        //#if (!no-tutorial)
        // BepInEx also gives us a config file for easy configuration.
        // See https://lethal.wiki/dev/intermediate/custom-configs

        // We can apply our hooks here.
        // See https://lethal.wiki/dev/fundamentals/patching-code

        // Log our awake here so we can see it in LogOutput.log file
        //#endif
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
