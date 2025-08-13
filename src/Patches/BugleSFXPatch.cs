using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using BugleMaestro.Helpers;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{

    // 1. todo - override movement while playing? (arrow keys)

    [HarmonyPatch(nameof(BugleSFX.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(BugleSFX __instance)
    {
        __instance.item.gameObject.AddComponent<BugleMaestroBehaviour>();
    }
    
    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip, float pitch)
    {
        // Set clip
        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();

        __instance.bugle = [ClipHelper.ChangePitch(mb.RPC_CurrentNote)]; // todo - pass in volume??

        __instance.currentClip = 0; // there is only one clip we want to play
        __instance.currentPitch = 1f;// pitch;

        // Logs
        var characterName = __instance.item?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {characterName} made a TOOOOOT! {mb.RPC_CurrentNote.ToString()}");
    }
    
    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        // Logging
        var characterName = __instance.item?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {characterName} ended toot now.");
    }

}