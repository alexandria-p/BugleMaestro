using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BugleMaestro.Helpers;
using UnityEngine.Android;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{
    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: TOOOOOT!");

        // Don't use elseif - or multikey inputs may not register properly.

        // Get Octave
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Lowest);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Highest);
        }
        if (!Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Neutral);
        }

        // Get Semitone
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Flat);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Sharp);
        }
        if (!Input.GetKeyDown(KeyCode.LeftArrow) && Input.GetKeyDown(KeyCode.RightArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Natural);
        }

        // Get Note/Pitch
        // Z -> M keys == C -> B notes
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.C);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.D);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.E);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.F);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.G);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.A);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ScaleHelper.SetNote(RawNoteInputEnum.B);
        }
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: End toot.");

        ScaleHelper.ResetToDefaultPitch();
    }

}