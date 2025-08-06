using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using Photon.Pun;
using Photon.Realtime;
using System;
using BugleMaestro.Helpers;
using UnityEngine;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{

    public RawNoteInputEnum CurrentRawNote { get; set; } = ScaleHelper.DEFAULT_RAW_NOTE;
    public bool IsANotePlaying { get; set; } = false;

    // BugleSFX.hold == holding note.



    // 1. todo - override movement while playing? (arrow keys)
    // 2. todo - make sure bugle is actually "playing" (for capybara achievement, and general appearance etc)
    // 3. work out our own Clip?

    [HarmonyPatch(nameof(BugleSFX.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(BugleSFX __instance)
    {
        __instance.item.gameObject.AddComponent<BugleMaestroBehaviour>();
    }

    [HarmonyPatch(nameof(BugleSFX.UpdateTooting))]
    [HarmonyPostfix]
    private static void UpdateTooting_Postfix(BugleSFX __instance)
    {        
        if (!__instance.photonView.IsMine)
        {
            return;
        }

        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();
        bool flag = mb.IsANotePlaying;
        if (flag && !__instance.hold)
        {
            int num = UnityEngine.Random.Range(0, __instance.bugle.Length);
            __instance.photonView.RPC("RPC_StartToot", RpcTarget.All, 1); // choose a clip (better yet, make our own)
        }
        else if (__instance.hold) // was held, but now should not be
        {
            __instance.photonView.RPC("RPC_EndToot", RpcTarget.All);
        }

        __instance.hold = flag;

    }

    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: TOOOOOT!");

        if (!__instance.photonView.IsMine)
        {
            return;
        }

        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();

        float[] pitch =
            [
                0.4f, 
                0.5f, 
                0.6f, 
                0.7f, 
                0.8f, 
                0.9f, 
                1f, 
            ];

        __instance.buglePlayer.pitch = pitch[(int)mb.CurrentRawNote];


        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: End toot.");
    }

}