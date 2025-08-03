using HarmonyLib;
using UnityEngine;
using BugleMaestro.Helpers;
using UnityEngine.UIElements;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{

    // todo - reset pitch when bugle unequipped. (low priority)
    // todo - override movement while playing?


    // 1. todo - tapping z -> m triggers toot, not just CLICK
    // 2. todo - if note changes (from prevnote to newnote) while playing, it starts toot again with new note

    [HarmonyPatch(nameof(BugleSFX.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(BugleSFX __instance)
    {
        // Don't use elseif - or multikey inputs may not register properly.

        // 3. todo - if already playing a note,
        // and octave or semitone changes,
        // update note & toot again
        // (requires checking whether a note was already sustained last frame compared to this frame)

        var prevFrameNote = (int)Plugin.Instance.CurrentNote;

        // Get Octave
        if (Input.GetKey(KeyCode.DownArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Lowest);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Highest);
        }
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            ScaleHelper.SetOctave(OctaveEnum.Neutral);
        }

        // Get Semitone
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Flat);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Sharp);
        }
        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            ScaleHelper.SetSemitoneModifier(SemitoneModifierEnum.Natural);
        }

        // Get Note/Pitch
        // Z -> M keys == C -> B notes

        int? currentFrameNote = null;


        if (Input.GetKey(KeyCode.Z))
        {
            UpdateNote(RawNoteInputEnum.C);
        }
        if (Input.GetKey(KeyCode.X))
        {
            UpdateNote(RawNoteInputEnum.D);
        }
        if (Input.GetKey(KeyCode.C))
        {
            UpdateNote(RawNoteInputEnum.E);
        }
        if (Input.GetKey(KeyCode.V))
        {
            UpdateNote(RawNoteInputEnum.F);
        }
        if (Input.GetKey(KeyCode.B))
        {
            UpdateNote(RawNoteInputEnum.G);
        }
        if (Input.GetKey(KeyCode.N))
        {
            UpdateNote(RawNoteInputEnum.A);
        }
        if (Input.GetKey(KeyCode.M))
        {
            UpdateNote(RawNoteInputEnum.B);
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

        void UpdateNote(RawNoteInputEnum rawNoteSustained)
        {
            // todo - if note change (octave/semitone) detected since last frame, re-toot with new note.
            var intermediaryNote = (int)ScaleHelper.RecalculateNote(rawNoteSustained);

            // either if note changes from prev Frame, or no note was playing and now there is one:
            //(Allows us to "walk" DOWN the scale)
            // if there are more than 2 notes pressed at a time, it will prioritise the highest pitch.
            if (!Plugin.Instance.IsNotePlaying || intermediaryNote != prevFrameNote)
            {
                currentFrameNote = intermediaryNote;
            }
        }

        // only want to TOOT once per Update, if there has been a change in note OR if no note was playing, and now there is.
        if (currentFrameNote != null)
        {
            Plugin.Instance.IsNotePlaying = true;
            ScaleHelper.Toot((ScaleEnum)currentFrameNote);
        }


        // todo - handle holding down 2 keys at a time, where it jumps back and forth between both: (make a list of "deadkeys" and remove from deadkey list once key is no longer being input???)
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_StartToot))]
    [HarmonyPostfix]
    private static void RPC_StartToot_Postfix(BugleSFX __instance, int clip)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: TOOOOOT!");

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: End toot.");
    }

}