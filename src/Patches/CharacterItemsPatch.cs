using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using Photon.Pun;
using Photon.Realtime;
using System;
using BugleMaestro.Helpers;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(CharacterItems))]
public class CharacterItemsPatch
{


    [HarmonyPatch(nameof(CharacterItems.DoUsing))]
    [HarmonyPrefix]
    private static bool DoUsing_Prefix(ref CharacterItems __instance)
    {
        if (__instance.character == null || __instance.character.data.currentItem == null)
        {
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

        bool flag = buglemb.IsANotePlaying;
        if (flag)
        {
            if (buglesfx.item.holderCharacter.data.currentItem.CanUsePrimary())
            {
                buglesfx.item.StartUsePrimary();
                return false;
            }
            if (buglesfx.item.holderCharacter.data.currentItem.CanUsePrimary())
            {
                buglesfx.item.ContinueUsePrimary();
                return false;
            }
        }
        else
        {
            buglesfx.item.CancelUsePrimary();
            return false;
        }
        // todo - handle changing notes? (stop, then start)


        return true;

        void CancelBugle()
        {
            buglesfx.item.CancelUsePrimary();
        }
    }
}