using HarmonyLib;
using UnityEngine;
using BugleMaestro.MonoBehaviors;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(CharacterMovement))]
public class CharacterMovementPatch
{
    [HarmonyPatch(nameof(CharacterMovement.SetMovementState))]
    [HarmonyPrefix]
    private static bool SetMovementState_Prefix(ref CharacterMovement __instance)
    {
        if (!__instance.character.refs.view.IsMine)
        {
            return true;
        }

        var heldItem = __instance.character?.data?.currentItem;

        if (__instance.character == null || heldItem == null || !heldItem.TryGetComponent(out BugleMaestroBehaviour mb) 
            || !mb.RPC_IsANoteInputBeingPressedByThePlayer)
        {
            // early exit if not holding a bugle, or if user is not currently blowing their bugle
            return true;
        }


        // reset movement input, if user is playing the bugle & is using only their arrow keys to move
        var movementBindingOtherThanArrowsArePressed = false;

        foreach (var binding in CharacterInput.action_move.bindings)
        {
            // Resolve the actual control
            var control = InputSystem.FindControl(binding.path);

            if (control is ButtonControl button && button.isPressed)
            {
                if (binding.path != "<Keyboard>/upArrow" &&
                    binding.path != "<Keyboard>/downArrow" &&
                    binding.path != "<Keyboard>/leftArrow" &&
                    binding.path != "<Keyboard>/rightArrow")
                {
                    movementBindingOtherThanArrowsArePressed = true;
                }
            }
        }

        // reset movement input
        if (!movementBindingOtherThanArrowsArePressed)
        {
            __instance.character.input.movementInput = Vector2.zero;
        }

        //  continue as normal
        return true;
    }

}