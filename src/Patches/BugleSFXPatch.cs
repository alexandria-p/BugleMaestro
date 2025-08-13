using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using Photon.Pun;
using Photon.Realtime;
using System;
using BugleMaestro.Helpers;
using UnityEngine;
using System.Reflection;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{
    // BugleSFX.hold == holding note.

    // 1. todo - override movement while playing? (arrow keys)


    // From Virtuoso [mondash] - Mark as already tooting so audio isn't overwritten in Update

    private static void SetTooting(BugleSFX instance) =>
        instance
            .GetType()
            .GetField("t", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(instance, true);



    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPrefix]
    private static bool RPC_StartToot_Prefix(ref BugleSFX __instance, int clip, float pitch)
    {

        //SetTooting(__instance);

        // Set clip
        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();
        __instance.buglePlayer.clip = ClipHelper.RandomClip(mb.RPC_CurrentNote);


        // regular code: TODO - can I just let this play out as usual instead??
        __instance.hold = true;
        if ((bool)__instance.particle1 && (bool)__instance.particle2)
        {
            if (!__instance.particle1.isPlaying)
            {
                __instance.particle1.Play();
            }
            if (!__instance.particle2.isPlaying)
            {
                __instance.particle2.Play();
            }
            ParticleSystem.EmissionModule emission = __instance.particle1.emission;
            ParticleSystem.EmissionModule emission2 = __instance.particle2.emission;
            emission.enabled = true;
            emission2.enabled = true;
        }

        // Logs
        var characterName = __instance.item?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {characterName} made a TOOOOOT! {mb.RPC_CurrentNote.ToString()}");

        // do not continue - override base function entirely.
        return false;
    }


    [HarmonyPatch(nameof(BugleSFX.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(BugleSFX __instance)
    {
        __instance.item.gameObject.AddComponent<BugleMaestroBehaviour>();
    }
    /*
    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip, float pitch)
    {
        // Set clip
        //SetTooting(__instance);
        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();
        __instance.buglePlayer.clip = ClipHelper.RandomClip(mb.RPC_CurrentNote);

        __instance.hold = true;


        // regular code: TODO - can I just let this play out as usual instead??
        __instance.hold = true;
        if ((bool)__instance.particle1 && (bool)__instance.particle2)
        {
            if (!__instance.particle1.isPlaying)
            {
                __instance.particle1.Play();
            }
            if (!__instance.particle2.isPlaying)
            {
                __instance.particle2.Play();
            }
            ParticleSystem.EmissionModule emission = __instance.particle1.emission;
            ParticleSystem.EmissionModule emission2 = __instance.particle2.emission;
            emission.enabled = true;
            emission2.enabled = true;
        }

        // Logs
        var characterName = __instance.item?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {characterName} made a TOOOOOT! {mb.RPC_CurrentNote.ToString()}");
    }
    */
    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        // regular code: TODO - can I just let this play out as usual instead??
        __instance.hold = false;
        if ((bool)__instance.particle1 && (bool)__instance.particle2)
        {
            ParticleSystem.EmissionModule emission = __instance.particle1.emission;
            ParticleSystem.EmissionModule emission2 = __instance.particle2.emission;
            emission.enabled = false;
            emission2.enabled = false;
        }

        // Logs
        var characterName = __instance.item?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {characterName} ended toot now.");
    }

}