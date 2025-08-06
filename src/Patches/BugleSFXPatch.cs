using HarmonyLib;
using BugleMaestro.MonoBehaviors;
using Photon.Pun;
using Photon.Realtime;
using System;
using BugleMaestro.Helpers;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace BugleMaestro.Patches;

[HarmonyPatch(typeof(BugleSFX))]
public class BugleSFXPatch
{

    private const float Frequency = 58.27f; // Bb1
    private static AudioClip? _clip;

    public static AudioClip Brass()
    {
        const int cycles = 10;
        const int sampleRate = 44100;
        const int sampleCount = (int)(sampleRate * cycles / Frequency);
        var samples = new float[sampleCount];

        const int harmonics = 20;

        // Formant boost range (approx. 1200–2500 Hz)
        // At Bb1, this is roughly harmonics 5–7
        const int formantMin = 5;
        const int formantMax = 7;

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var basePhase = 2 * Mathf.PI * Frequency * t;

            var value = 0f;

            for (var n = 1; n <= harmonics; n++)
            {
                var harmonicPhase = basePhase * n;

                // Soft pinch shaping (nonlinear — richer than pure sine)
                var sine = Mathf.Sin(harmonicPhase);
                var shaped = Mathf.Sign(sine) * Mathf.Pow(Mathf.Abs(sine), 1.1f); // pinch amount

                // Harmonic weight
                var weight = 1f / n * Mathf.Exp(-0.045f * n);

                // Formant bump around harmonics 5–7
                if (n is >= formantMin and <= formantMax) weight *= 1.5f;

                value += weight * shaped;
            }

            // Mild saturation to simulate horn compression (adds buzz edge)
            value += 0.15f * Mathf.Pow(value, 3f);

            // Slight breath noise
            var breath = 0.005f * (UnityEngine.Random.value - 0.5f);
            samples[i] = value * 0.3f + breath;
        }

        _clip = AudioClip.Create("BuglePinchedClip", sampleCount, 1, sampleRate, false);
        _clip.SetData(samples, 0);
        return _clip;
    }



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

        /*
         * if (!__instance.photonView.IsMine)
        {
            return;
        }
        var mb = __instance.item.gameObject.GetComponent<BugleMaestroBehaviour>();
        bool flag = mb.IsANotePlaying;

        if (__instance.hold && !flag) //was playing -> now stop playing,
        {
            //__instance.item.CancelUsePrimary(); - let characteritems patch handle this?
            //__instance.photonView.RPC("RPC_EndToot", RpcTarget.All);
        }

        // todo - catch if note has changed --> now stop playing

        if (flag && !__instance.hold)
        {
            if (!__instance.item.holderCharacter.data.passedOut && !__instance.item.holderCharacter.data.fullyPassedOut)
            {
                if (__instance.item.holderCharacter.input.usePrimaryWasPressed && __instance.item.holderCharacter.data.currentItem.CanUsePrimary())
                {
                    __instance.item.StartUsePrimary();
                }
            }

            //int num = UnityEngine.Random.Range(0, __instance.bugle.Length);
            //__instance.photonView.RPC("RPC_StartToot", RpcTarget.All, 1); // choose a clip (better yet, make our own)
        }
        if (flag && __instance.hold)
        {
            if (!__instance.item.holderCharacter.data.passedOut && !__instance.item.holderCharacter.data.fullyPassedOut)
            {
                if (__instance.item.holderCharacter.input.usePrimaryWasPressed && __instance.item.holderCharacter.data.currentItem.CanUsePrimary())
                {
                    __instance.item.ContinueUsePrimary();
                }
            }

            //int num = UnityEngine.Random.Range(0, __instance.bugle.Length);
            //__instance.photonView.RPC("RPC_StartToot", RpcTarget.All, 1); // choose a clip (better yet, make our own)
        }

        __instance.hold = flag;
        */
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
            0.2f, 
            0.37f, 
            0.54f, 
            0.71f, 
            0.88f, 
            1.05f, 
            1.22f, 
        ];

        __instance.buglePlayer.clip = Brass();
        __instance.currentClip = 0;
        __instance.buglePlayer.pitch = pitch[(int)mb.CurrentRawNote];

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Set Note: {Plugin.Instance.CurrentNote.ToString()}");
    }

    [HarmonyPatch(nameof(BugleSFX.RPC_EndToot))]
    [HarmonyPostfix]
    private static void RPC_EndToot_Postfix(BugleSFX __instance)
    {
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: End toot now.");
    }

}