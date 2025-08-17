using BugleMaestro.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

namespace BugleMaestro.MonoBehaviors;


// remember - this monobehaviour will be added to the bugle gameobject
internal class BugleMaestroBehaviour : MonoBehaviourPun
{
    private static Item? _bugleItemInstance;

    // Sync these variables over RPC
    // so if these variables are synced over RPC,
    // they will be correct for each bugle on every client
    public bool IsPlaying => RPC_IsANoteInputBeingPressedByThePlayer;
    public ScaleEnum RPC_CurrentNote { get; private set; } = ScaleHelper.DEFAULT_NOTE;
    public bool RPC_IsANoteInputBeingPressedByThePlayer { get; private set; } = false;
    public bool IsANewNoteChangePendingForLocalPlayer { get; private set; } = false; // changes to TRUE to trigger changes on every client, which will change the local value back to FALSE once complete on their client.

    // all tracked locally
    public RawNoteInputEnum Local_CurrentRawNote { get; set; } = ScaleHelper.DEFAULT_RAW_NOTE;
    private OctaveEnum Local_CurrentOctave { get; set; } = ScaleHelper.DEFAULT_OCTAVE;
    private SemitoneModifierEnum Local_CurrentSemitoneModifier { get; set; } = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    
    // Track inputs
    private HashSet<OctaveEnum> Local_LastFrameOctaveInput { get; set; } = new HashSet<OctaveEnum>();
    private HashSet<SemitoneModifierEnum> Local_LastFrameSemitoneInput { get; set; } = new HashSet<SemitoneModifierEnum>();
    private List<RawNoteInputEnum> Local_LastFrameRawNoteInput { get; set; } = new List<RawNoteInputEnum>(); // list to preserve insertion order


    private void LocalPlayerSetsNote(ScaleEnum newNote)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // triggers the below method to play out on every client
        photonView.RPC(nameof(RPC_UpdateNotePlaying), RpcTarget.All, newNote);
    }

    [PunRPC] // updates these details on every client
    private void RPC_UpdateNotePlaying(ScaleEnum newNote)
    {
        if (!RPC_IsANoteInputBeingPressedByThePlayer || RPC_CurrentNote != newNote)
        {
            RPC_CurrentNote = newNote;
            IsANewNoteChangePendingForLocalPlayer = true;
        }

        RPC_IsANoteInputBeingPressedByThePlayer = true;

        var playerName = _bugleItemInstance?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX} (To everyone): {playerName} Set Note: {RPC_CurrentNote.ToString()}");
    }

    private void LocalStopInput()
    {
        if (!photonView.IsMine || !IsPlaying)
        {
            return;
        }

        // triggers the below method to play out on every client
        photonView.RPC(nameof(RPC_StopNotePlaying), RpcTarget.All);
    }

    [PunRPC] // updates these details on every client
    private void RPC_StopNotePlaying()
    {
        if (IsPlaying)
        {
            //var playerName = _bugleItemInstance?.holderCharacter?.characterName ?? Plugin.DEFAULT_CHARACTER_NAME;
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX} (To everyone): {playerName} Stopped playing their bugle");
            ResetBugleState();
        }
    }

    public void ResetBugleState()
    {
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Reset bugle state");
        IsANewNoteChangePendingForLocalPlayer = false;
        ResetToDefaultPitch();
        RPC_CurrentNote = ScaleHelper.DEFAULT_NOTE;
        RPC_IsANoteInputBeingPressedByThePlayer = false;
    }

    public void UpdateLocalIsNotePending(bool newValue)
    {
        IsANewNoteChangePendingForLocalPlayer = newValue;
    }

    private void Awake()
    {
        if (_bugleItemInstance) return;
        _bugleItemInstance = gameObject.GetComponent<BugleSFX>()?.item;

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: BugleMaestroBehaviour Awake");
    }

    private void Update()
    {
        if (_bugleItemInstance == null) 
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: null issue");
            _bugleItemInstance = gameObject.GetComponent<BugleSFX>()?.item; // attempt to set bugle instance
            return;
        }

        // Reset the bugle state if necessary (e.g. if dropped, or if item.CancelUsePrimary() was called)
        // (then continue with the rest of the method)
        if (!_bugleItemInstance.isUsingPrimary)
        {
            if (IsPlaying)
            {
                ResetBugleState();
            }            
        }

        // early exit if the local player is not holding the bugle
        if (_bugleItemInstance.itemState != ItemState.Held)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Bugle not in player's hands");
            return;
        }
        if (_bugleItemInstance.holderCharacter == null || !_bugleItemInstance.holderCharacter.IsLocal)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Bugle not being held by local player");
            return;
        }

        
        

        // Now, the LOCAL PLAYER is the one actively holding the bugle in their hands.


        
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
            LocalStopInput();
            return; // early exit.
        }

        RawNoteInputEnum? newTootRawNote = FindNewTootRawNote();
        if (newTootRawNote != null)
        {
            SetRawNote(newTootRawNote.Value);
            var noteOnScale = ScaleHelper.CalculateScaleNote(Local_CurrentRawNote, Local_CurrentOctave, Local_CurrentSemitoneModifier);
            LocalPlayerSetsNote(noteOnScale);
        }

        // don't update key tracking until after calculations.
        UpdateKeyInputTracking();

        void UpdateKeyInputTracking()
        {
            Local_LastFrameOctaveInput.RemoveWhere(_ => !octavesBeingPressedThisFrame.Contains(_));
            Local_LastFrameSemitoneInput.RemoveWhere(_ => !semitonesBeingPressedThisFrame.Contains(_));
            Local_LastFrameRawNoteInput.RemoveAll(_ => !rawNotesBeingPressedThisFrame.Contains(_));

            // no need to do an 'if' check before adding, as hashset are unique elements.
            foreach (var keypress in octavesBeingPressedThisFrame)
                Local_LastFrameOctaveInput.Add(keypress);

            foreach (var keypress in semitonesBeingPressedThisFrame)
                Local_LastFrameSemitoneInput.Add(keypress);

            foreach (var keypress in rawNotesBeingPressedThisFrame)
            {
                if (!Local_LastFrameRawNoteInput.Contains(keypress)) // cause this is a list to preserve insertion order
                {
                    Local_LastFrameRawNoteInput.Add(keypress);
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
                if (!RPC_IsANoteInputBeingPressedByThePlayer ||
                    !Local_LastFrameRawNoteInput.Contains(rawNote))
                // always play a note if there were none last frame, and now there is on this frame:
                // OR always play the note if this is a new rawnote keypress (was not held last frame)
                {
                    return rawNote;
                }
            }

            // if we are still here, then we didn't find any new raw keypresses to play.
            // so let's see if there was an octave or semitone change this frame, that necessitates a new note.

            if (Local_LastFrameOctaveInput.SetEquals(octavesBeingPressedThisFrame)
                && Local_LastFrameSemitoneInput.SetEquals(semitonesBeingPressedThisFrame)
                )
            {
                // potentially - the octave/semitone is the same but user releases one of multiple keys
                // so lets recalculate notes.
                // early exit with no change if the note sustained last frame is still being held:
                if (rawNotesBeingPressedThisFrame.Contains(Local_CurrentRawNote))
                {
                    return null; // early exit with no new note.
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (Local_LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = Local_LastFrameRawNoteInput[i];
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
                if (rawNotesBeingPressedThisFrame.Contains(Local_CurrentRawNote))
                {
                    return Local_CurrentRawNote;
                }

                // (otherwise....notes are added to the LastFrameRawNoteInput list in the order they were pressed ("lists preserve insertion order"). Let's go back to the most recent press.)
                for (var i = (Local_LastFrameRawNoteInput.Count) - (1); i >= 0; i--)
                {
                    var key = Local_LastFrameRawNoteInput[i];
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
    
    private void SetOctave(OctaveEnum newOctave)
    {
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set octave: {newOctave.ToString()}");
        if (Local_CurrentOctave != newOctave)
        {
            Local_CurrentOctave = newOctave;
        }
    }

    private void SetSemitoneModifier(SemitoneModifierEnum newModifier)
    {
        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set semitone: {newModifier.ToString()}");

        if (Local_CurrentSemitoneModifier != newModifier)
        {
            Local_CurrentSemitoneModifier = newModifier;
        }
    }

    private void SetRawNote(RawNoteInputEnum newNote)
    {
        if (Local_CurrentRawNote != newNote)
        {
            Local_CurrentRawNote = newNote;
        }

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }

    private void ResetToDefaultPitch()
    {
        Local_CurrentRawNote = ScaleHelper.DEFAULT_RAW_NOTE;
        Local_CurrentOctave = ScaleHelper.DEFAULT_OCTAVE;
        Local_CurrentSemitoneModifier = ScaleHelper.DEFAULT_SEMITONE_MODIFIER;
    }
}