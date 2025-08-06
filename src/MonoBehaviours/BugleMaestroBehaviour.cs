using BugleMaestro;
using BugleMaestro.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace BugleMaestro.MonoBehaviors;



internal class BugleMaestroBehaviour : MonoBehaviourPun
{
    private static Item? _bugleItemInstance;

    // todo - sync these variables over RPC
    public ScaleEnum CurrentNote { get; set; } = ScaleHelper.DEFAULT_NOTE;
    public bool IsANotePlaying { get; set; } = false;

    // all set locally
    public RawNoteInputEnum CurrentRawNote { get; set; } = ScaleHelper.DEFAULT_RAW_NOTE;
    private OctaveEnum CurrentOctave { get; set; } = ScaleHelper.DEFAULT_OCTAVE;
    private SemitoneModifierEnum CurrentSemitoneModifier { get; set; } = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    
    // Track inputs
    private HashSet<OctaveEnum> LastFrameOctaveInput { get; set; } = new HashSet<OctaveEnum>();
    private HashSet<SemitoneModifierEnum> LastFrameSemitoneInput { get; set; } = new HashSet<SemitoneModifierEnum>();
    private List<RawNoteInputEnum> LastFrameRawNoteInput { get; set; } = new List<RawNoteInputEnum>(); // list to preserve insertion order


    // this is the one we care about syncing over punRPC!
    private void SetNote(ScaleEnum newNote)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        if (CurrentNote != newNote)
        {
            CurrentNote = newNote;
        }

        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {CurrentNote.ToString()}");
    }

    

    private void Awake()
    {
        if (_bugleItemInstance) return;
        _bugleItemInstance = gameObject.GetComponent<BugleSFX>()?.item;

        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: BugleMaestroBehaviour Awake");

        //GlobalEvents.OnBugleTooted = (Action<Item>)Delegate.Combine(GlobalEvents.OnBugleTooted, new Action<Item>(TestBugleTooted));
    }

    private void Update()
    {
        if (_bugleItemInstance == null) 
        {
            Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: null issue");
            _bugleItemInstance = gameObject.GetComponent<BugleSFX>()?.item; // attempt to set bugle instance
            return;
        };
        if (_bugleItemInstance.holderCharacter == null || !_bugleItemInstance.holderCharacter.IsLocal)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Bugle not being held by local player");
            return;
        }
        if (_bugleItemInstance.itemState != ItemState.Held)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Bugle not in player's hands");
            return;
        }

        // Now, the local player is the one actively holding the bugle in their hands.


        
        // Don't use elseif - or multikey inputs may not register properly.

        HashSet<OctaveEnum> octavesBeingPressedThisFrame = new();
        HashSet<SemitoneModifierEnum> semitonesBeingPressedThisFrame = new();
        HashSet<RawNoteInputEnum> rawNotesBeingPressedThisFrame = new();

        // Get Octave
        if (Input.GetKey(KeyCode.DownArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Lowest);
            SetOctave(OctaveEnum.Lowest);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Highest);
            SetOctave(OctaveEnum.Highest);
        }
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            octavesBeingPressedThisFrame.Add(OctaveEnum.Neutral);
            SetOctave(OctaveEnum.Neutral);
        }

        // Get Semitone
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Flat);
            SetSemitoneModifier(SemitoneModifierEnum.Flat);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Sharp);
            SetSemitoneModifier(SemitoneModifierEnum.Sharp);
        }
        if (!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            semitonesBeingPressedThisFrame.Add(SemitoneModifierEnum.Natural);
            SetSemitoneModifier(SemitoneModifierEnum.Natural);
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
            StopPlaying();
        }

        RawNoteInputEnum? newTootRawNote = FindNewTootRawNote();
        if (newTootRawNote != null)
        {
            IsANotePlaying = true;
            SetRawNote(newTootRawNote.Value);
            var result = ScaleHelper.CalculateScaleNote(CurrentRawNote, CurrentOctave, CurrentSemitoneModifier);
            SetNote(result);
            //Toot(CurrentNote);
        }

        // don't update key tracking until after calculations.
        UpdateKeyInputTracking();

        void UpdateKeyInputTracking()
        {
            LastFrameOctaveInput.RemoveWhere(_ => !octavesBeingPressedThisFrame.Contains(_));
            LastFrameSemitoneInput.RemoveWhere(_ => !semitonesBeingPressedThisFrame.Contains(_));
            LastFrameRawNoteInput.RemoveAll(_ => !rawNotesBeingPressedThisFrame.Contains(_));

            // no need to do an 'if' check before adding, as hashset are unique elements.
            foreach (var keypress in octavesBeingPressedThisFrame)
                LastFrameOctaveInput.Add(keypress);

            foreach (var keypress in semitonesBeingPressedThisFrame)
                LastFrameSemitoneInput.Add(keypress);

            foreach (var keypress in rawNotesBeingPressedThisFrame)
            {
                if (!LastFrameRawNoteInput.Contains(keypress)) // cause this is a list to preserve insertion order
                {
                    LastFrameRawNoteInput.Add(keypress);
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
                if (!IsANotePlaying ||
                    !LastFrameRawNoteInput.Contains(rawNote))
                // always play a note if there were none last frame, and now there is on this frame:
                // OR always play the note if this is a new rawnote keypress (was not held last frame)
                {
                    return rawNote;
                }
            }

            // if we are still here, then we didn't find any new raw keypresses to play.
            // so let's see if there was an octave or semitone change this frame, that necessitates a new note.

            if (LastFrameOctaveInput.SetEquals(octavesBeingPressedThisFrame)
                && LastFrameSemitoneInput.SetEquals(semitonesBeingPressedThisFrame)
                )
            {
                // potentially - the octave/semitone is the same but user releases one of multiple keys
                // so lets recalculate notes.
                // early exit with no change if the note sustained last frame is still being held:
                if (rawNotesBeingPressedThisFrame.Contains(CurrentRawNote))
                {
                    return null; // early exit with no new note.
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = LastFrameRawNoteInput[i];
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
                if (rawNotesBeingPressedThisFrame.Contains(CurrentRawNote))
                {
                    return CurrentRawNote;
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = LastFrameRawNoteInput[i];
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

    private void StopPlaying()
    {
        if (IsANotePlaying)
        {
            Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Stopped playing");
            IsANotePlaying = false;
            ResetToDefaultPitch();
        }
    }

    private void ResetToDefaultPitch()
    {
        CurrentNote = ScaleHelper.DEFAULT_NOTE;
        CurrentRawNote = ScaleHelper.DEFAULT_RAW_NOTE;
        CurrentOctave = ScaleHelper.DEFAULT_OCTAVE;
        CurrentSemitoneModifier = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    }

    private void SetOctave(OctaveEnum newOctave)
    {
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set octave: {newOctave.ToString()}");
        if (CurrentOctave != newOctave)
        {
            CurrentOctave = newOctave;
        }
    }

    private void SetSemitoneModifier(SemitoneModifierEnum newModifier)
    {
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set semitone: {newModifier.ToString()}");

        if (CurrentSemitoneModifier != newModifier)
        {
            CurrentSemitoneModifier = newModifier;
        }
    }

    private void SetRawNote(RawNoteInputEnum newNote)
    {
        if (CurrentRawNote != newNote)
        {
            CurrentRawNote = newNote;
        }

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }
}