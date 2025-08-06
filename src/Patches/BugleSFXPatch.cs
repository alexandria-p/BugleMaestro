using HarmonyLib;
using UnityEngine;
using BugleMaestro.Helpers;
using System.Collections.Generic;
using System.Linq;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{

    // todo - reset pitch when bugle unequipped. (low priority)
    // todo - override movement while playing?


    // 1. todo - the TOOT method creates a new toot (not just clicking now)
    // 2. todo - only run this update method to calculate keychange and TOOTS if bugle is held by player

    [HarmonyPatch(nameof(BugleSFX.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(BugleSFX __instance)
    {
        // Don't use elseif - or multikey inputs may not register properly.

        HashSet<OctaveEnum> octavesBeingPressedThisFrame = new();
        HashSet<SemitoneModifierEnum> semitonesBeingPressedThisFrame = new();
        HashSet<RawNoteInputEnum> rawNotesBeingPressedThisFrame = new();

        // Get Octave
        if (Input.GetKey(KeyCode.DownArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Lowest);
            ScaleHelper.SetOctave(OctaveEnum.Lowest);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Highest);
            ScaleHelper.SetOctave(OctaveEnum.Highest);
        }
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Neutral);
            ScaleHelper.SetOctave(OctaveEnum.Neutral);
        }

        // Get Semitone
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Flat);
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Flat);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Sharp);
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Sharp);
        }
        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Natural);
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Natural);
        }

        // Get Note/Pitch
        // Z -> M keys == C -> B notes

        if (Input.GetKey(KeyCode.Z))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.C);
        }
        if (Input.GetKey(KeyCode.X))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.D);
        }
        if (Input.GetKey(KeyCode.C))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.E);
        }
        if (Input.GetKey(KeyCode.V))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.F);
        }
        if (Input.GetKey(KeyCode.B))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.G);
        }
        if (Input.GetKey(KeyCode.N))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.A);
        }
        if (Input.GetKey(KeyCode.M))
        {
            rawNotesBeingPressedThisFrame.Add(RawNoteInputEnum.B);
        }
        if (!Input.GetKey(KeyCode.Z)
            && !Input.GetKey(KeyCode.X)
            && !Input.GetKey(KeyCode.C)
            && !Input.GetKey(KeyCode.V)
            && !Input.GetKey(KeyCode.B)
            && !Input.GetKey(KeyCode.N)
            && !Input.GetKey(KeyCode.M))
        {
            ScaleHelper.ResetToDefaultPitch();
        }

        RawNoteInputEnum? newTootRawNote = FindNewTootRawNote();
        if (newTootRawNote != null)
        {
            Plugin.Instance.IsANotePlaying = true;
            ScaleHelper.SetRawNote(newTootRawNote.Value);            
            ScaleHelper.RecalculateAndSetCurrentNote();
            ScaleHelper.Toot(Plugin.Instance.CurrentNote);
        }

        // don't update key tracking until after calculations.
        UpdateKeyInputTracking();

        void UpdateKeyInputTracking()
        {
            Plugin.Instance.LastFrameOctaveInput.RemoveWhere(_ => !octavesBeingPressedThisFrame.Contains(_));
            Plugin.Instance.LastFrameSemitoneInput.RemoveWhere(_ => !semitonesBeingPressedThisFrame.Contains(_));
            Plugin.Instance.LastFrameRawNoteInput.RemoveAll(_ => !rawNotesBeingPressedThisFrame.Contains(_));

            // no need to do an 'if' check before adding, as hashset are unique elements.
            foreach (var keypress in octavesBeingPressedThisFrame)
                Plugin.Instance.LastFrameOctaveInput.Add(keypress);

            foreach (var keypress in semitonesBeingPressedThisFrame)
                Plugin.Instance.LastFrameSemitoneInput.Add(keypress);

            foreach (var keypress in rawNotesBeingPressedThisFrame)
            {
                if (!Plugin.Instance.LastFrameRawNoteInput.Contains(keypress)) // cause this is a list to preserve insertion order
                {
                    Plugin.Instance.LastFrameRawNoteInput.Add(keypress);
                }                
            }
        }

        RawNoteInputEnum? FindNewTootRawNote()
        {
            if (!rawNotesBeingPressedThisFrame.Any())
            {
                return null; // early exit
            }

            foreach (var rawNote in rawNotesBeingPressedThisFrame)
            {
                // early exit from foreach loop if you find a valid new note to TOOT.
                if (!Plugin.Instance.IsANotePlaying || 
                    !Plugin.Instance.LastFrameRawNoteInput.Contains(rawNote))
                // always play a note if there were none last frame, and now there is on this frame:
                // OR always play the note if this is a new rawnote keypress (was not held last frame)
                {
                    return rawNote;
                }
            }

            // if we are still here, then we didn't find any new raw keypresses to play.
            // so let's see if there was an octave or semitone change this frame, that necessitates a new note.
            
            if (Plugin.Instance.LastFrameOctaveInput.SetEquals(octavesBeingPressedThisFrame)
                && Plugin.Instance.LastFrameSemitoneInput.SetEquals(semitonesBeingPressedThisFrame)
                )
            {
                // potentially - the octave/semitone is the same but user releases one of multiple keys
                // so lets recalculate notes.
                // early exit with no change if the note sustained last frame is still being held:
                if (rawNotesBeingPressedThisFrame.Contains(Plugin.Instance.CurrentRawNote))
                {
                    return null; // early exit with no new note.
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (Plugin.Instance.LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = Plugin.Instance.LastFrameRawNoteInput[i];
                    if (rawNotesBeingPressedThisFrame.Contains(key))
                    {
                        return key;
                    }
                }
            }
            // if note change (octave/semitone) detected since last frame, re-toot with new note.
            else if (rawNotesBeingPressedThisFrame.Any())
            {
                // else - there has been a change in octave or semitone. 
                // so lets recalculate notes.
                // let's try to use the note sustained last frame, if it is still being held:
                if (rawNotesBeingPressedThisFrame.Contains(Plugin.Instance.CurrentRawNote))
                {
                    return Plugin.Instance.CurrentRawNote;
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (Plugin.Instance.LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = Plugin.Instance.LastFrameRawNoteInput[i];
                    if (rawNotesBeingPressedThisFrame.Contains(key))
                    {
                        return key; 
                    }
                }
            }

            //fall-through
            return null;
        }        
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: TOOOOOT!");

        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: End toot.");
    }

}