using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using BugleMaestro.Patches;

namespace BugleMaestro;

// This BepInAutoPlugin attribute comes from the Hamunii.BepInEx.AutoPlugin NuGet package
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;

    internal static ManualLogSource Log { get; private set; } = null!;
    private Harmony? _harmonyInstance;
    public readonly static string LOG_PREFIX = "BugleMaestro";
    public readonly static string DEFAULT_CHARACTER_NAME = "DEFAULT_NAME";
    
    private void Awake()
    {
        Instance = this;
        Log = Logger;


        //_harmonyInstance = new Harmony(Info.Metadata.GUID).PatchAll(); // Info.Metadata.GUID // "com.github.PEAKModding.AlexModTest"
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(BugleSFXPatch)); // works
        _harmonyInstance = Harmony.CreateAndPatchAll(typeof(CharacterItemsPatch));
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
