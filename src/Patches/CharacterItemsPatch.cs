using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using BugleMaestro.Helpers;
using UnityEngine;

namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(CharacterItems))]
public class CharacterItemsPatch
{
    [HarmonyPatch(nameof(CharacterItems.DropItemRpc))]
    [HarmonyPrefix]
    private static bool DropItemRpc_Prefix(ref CharacterItems __instance, float throwCharge, byte slotID, Vector3 spawnPos, Vector3 velocity, Quaternion rotation, ItemInstanceData itemInstanceData)
    {
        var currentItem = __instance.character.data.currentItem;
        var buglemb = currentItem?.gameObject.GetComponent<BugleMaestroBehaviour>();
        var buglesfx = currentItem?.gameObject.GetComponent<BugleSFX>();

        // if not dropping bugle, continue
        if (!currentItem || buglemb == null || buglesfx == null)
        {
            return true;
        }

        // If local player is the Maestro, hide UI
        if (buglesfx.item.holderCharacter.IsLocal)
        {
            UIHelper.HideUI();
        }

        //  continue as normal
        return true;
    }


    // The idea here is that we tap into the 'item.isUsingPrimary' functionality that is already happening, so that our mod has minimal impact on the game logic.
    [HarmonyPatch(nameof(CharacterItems.DoUsing))]
    [HarmonyPrefix]
    private static bool DoUsing_Prefix(ref CharacterItems __instance)
    {
        if (__instance == null)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: instance is null");

            //  continue as normal
            return true;
        }

        if (__instance.character == null || __instance.character.data == null || __instance.character.data.currentItem == null)
        {
            //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: character null={__instance.character == null}, characterData null={__instance.character?.data == null}, currentItem null={__instance.character?.data?.currentItem == null}");

            //  continue as normal
            return true;
        }
        var buglemb = __instance.character.data.currentItem.gameObject.GetComponent<BugleMaestroBehaviour>();
        var buglesfx = __instance.character.data.currentItem.gameObject.GetComponent<BugleSFX>();
        if (buglemb == null || buglesfx == null)
        {
            // continue as normal
            return true;
        }

        if (buglesfx.item.holderCharacter.data.passedOut || (buglesfx.item.holderCharacter.data.fullyPassedOut))
        {
            CancelBugle();
            // continue as normal
            return true;
        }

        // if we get here, then 
        // - character is holding the bugle and is able to play it

       
        bool isNoteInputHeldByUser = buglemb.RPC_IsANoteInputBeingPressedByThePlayer;
        bool bugleItemIsInUse = buglesfx.item.isUsingPrimary;  // - detect HOLD

        // handle changing notes (stop, then start)
        if (buglemb.IsANewNoteChangePendingForLocalPlayer && bugleItemIsInUse)
        {
            CancelBugle(); // stop, so that a new note can start.
        }

        
        if (isNoteInputHeldByUser)
        {
            if (!bugleItemIsInUse && buglesfx.item.holderCharacter.data.currentItem.CanUsePrimary())
            {
                //character.input.usePrimaryIsPressed
                StartBugle();
                return false;
            }
            if (bugleItemIsInUse && buglesfx.item.holderCharacter.data.currentItem.CanUsePrimary())
            {
                ContinueBugle();
                return false;
            }
        }
        else if (!isNoteInputHeldByUser && bugleItemIsInUse) // else, user was blowing the bugle last frame but is no longer holding a note, so cancel its use.
        {
            CancelBugle();
            return false;
        }

        // fall-through to the regular method
        return true;

        void StartBugle()
        {
            // If local player is the Maestro, display UI
            if (buglesfx.item.holderCharacter.IsLocal)
            {
                UIHelper.DisplayBugleNote(buglemb.RPC_CurrentNote);
            }
            
            // Trigger Bugle start
            buglesfx.item.StartUsePrimary();
            buglemb.UpdateLocalIsNotePending(false);
        }

        void ContinueBugle()
        {
            buglesfx.item.ContinueUsePrimary();
        }

        void CancelBugle()
        {
            // If local player is the Maestro, hide UI
            if (buglesfx.item.holderCharacter.IsLocal)
            {
                UIHelper.HideUI();
            }

            buglesfx.item.CancelUsePrimary();
        }
    }
}