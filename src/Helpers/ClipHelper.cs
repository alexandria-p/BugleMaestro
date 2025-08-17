using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using BepInEx;
using System.IO;

namespace BugleMaestro.Helpers;

public class ClipHelper
{
    public static AudioClip ChangePitch(ScaleEnum targetScaleNote)
    {
        // early out if same as any base bugle clip without modification:
        foreach (var item in Plugin.Instance.baseBugleClips)
        {
            if (targetScaleNote == item.Key)
            {
                return item.Value;
            }
        }

        // else, find closest bugle Clip to modify (pitch-shift):

        // (use highest baseClip as default/fall-through)
        AudioClip? baseClipToModify = Plugin.Instance.baseBugleClips.GetValueOrDefault(ScaleHelper.HIGHEST_NOTE); 
        var numOfSemitoneDifferenceFromBaseClip = (int)targetScaleNote - (int)ScaleHelper.HIGHEST_NOTE;

        foreach (var item in Plugin.Instance.baseBugleClips)
        {
            // e.g. C5 - C#5 == 13 - 14 == -1
            // (-1 semitone difference between bugle C#5 to get to C5)
            var diff = (int)targetScaleNote - (int)item.Key;

            // each octave is 12 semitones, and we pretty much have one mp3 per octave to choose from
            if (diff <= 6)
            {
                // if you are 6(or less) semitones away to the nearest mp3 audioclip, then lets say *that one* is the closest 'base clip' for us to use and pitch-shift.
                baseClipToModify = item.Value;
                numOfSemitoneDifferenceFromBaseClip = diff;
                //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Pitch-shifting bugle clip {item.Key.ToString()} for target note {targetScaleNote.ToString()}");
                break;
            }
        }

         // shortcut - just use the AudioClip pitch function?
         return CreateNewAudioClipByPitchShifting(baseClipToModify, numOfSemitoneDifferenceFromBaseClip);
    }

    private static AudioClip CreateNewAudioClipByPitchShifting(AudioClip baseClipToModify, float numOfSemitoneDifferenceFromBaseClip)
    {
        float pitchFactor = Mathf.Pow(2f, (1f * numOfSemitoneDifferenceFromBaseClip) / 12f);
        int channels = baseClipToModify.channels;
        int newSamples = Mathf.CeilToInt(baseClipToModify.samples / pitchFactor);
        float[] data = new float[baseClipToModify.samples * channels];
        baseClipToModify.GetData(data, 0);

        float[] newData = new float[newSamples * channels];
        for (int i = 0; i < newSamples; i++)
        {
            float interpIndex = i * pitchFactor;
            int indexFloor = (int)interpIndex;
            int indexCeil = Mathf.Min(indexFloor + 1, baseClipToModify.samples - 1);

            float t = interpIndex - indexFloor;

            for (int c = 0; c < channels; c++)
            {
                float sampleFloor = data[indexFloor * channels + c];
                float sampleCeil = data[indexCeil * channels + c];
                newData[i * channels + c] = Mathf.Lerp(sampleFloor, sampleCeil, t);
            }
        }

        AudioClip newClip = AudioClip.Create(baseClipToModify.name + "_Pitched",
            newSamples, channels, baseClipToModify.frequency, false);
        newClip.SetData(newData, 0);
        return newClip;
    }
    

    public static async void SetupBaseBugleClips()
    {
        Dictionary<ScaleEnum, string> bugleClipFilepaths = new Dictionary<ScaleEnum, string>();
        bugleClipFilepaths.Add(ScaleEnum.C2, "c2_bugle.mp3");
        bugleClipFilepaths.Add(ScaleEnum.C3, "c3_bugle.mp3");
        bugleClipFilepaths.Add(ScaleEnum.C4, "c4_bugle.mp3");
        bugleClipFilepaths.Add(ScaleEnum.B4, "b4_bugle.mp3");

        foreach (var item in bugleClipFilepaths)
        {
            AudioClip result = await CreateAudioClipFromMp3(item.Value);
            Plugin.Instance.baseBugleClips.Add(item.Key, result);
        }
    }

    public static async Task<AudioClip> CreateAudioClipFromMp3(string filename)
    {
        string mp3Path = Path.Combine(Paths.PluginPath, $"{Plugin.TEAM_NAME}-{Plugin.MOD_NAME}", filename);
        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {Paths.PluginPath}");
        string url = "file:///" + mp3Path;

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Plugin.Log.LogError($"{Plugin.LOG_PREFIX}: Error loading {filename} MP3 from {mp3Path}. Check whether the MP3 file from the mod exists at this filepath. Full error: " + www.error);
        }

        // Get the original decoded clip
        AudioClip originalClip = DownloadHandlerAudioClip.GetContent(www);

        // Extract the samples
        float[] samples = new float[originalClip.samples * originalClip.channels];
        originalClip.GetData(samples, 0);

        // Create a new clip with the same settings but fully in memory
        AudioClip newClip = AudioClip.Create(
            "EditableBugleClip",
            originalClip.samples,
            originalClip.channels,
            originalClip.frequency,
            false // no streaming — fully loaded
        );

        // Copy the samples into the new clip
        newClip.SetData(samples, 0);

        Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: {filename} MP3 loaded into editable AudioClip with " +
                    newClip.samples + " samples, " +
                    newClip.channels + " channels, " +
                    newClip.frequency + " Hz");

        return newClip;
    }
}