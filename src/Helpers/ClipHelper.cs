using UnityEngine;

namespace BugleMaestro.Helpers;

public class ClipHelper
{
    // Reference for note frequencies: https://www.liutaiomottola.com/formulae/freqtab.htm
    public static AudioClip RandomClip(ScaleEnum scaleNote)
    {
        var random = Random.Range(0, 3);
        FrequencyAttribute frequencyAttr = ScaleHelper.GetAttributeOfScaleNote<FrequencyAttribute>(scaleNote);
        float frequency = frequencyAttr.Frequency;

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Generating bugle clip at frequency {frequency}Hz");

        return Pinched(frequency);
    }

    public static AudioClip Pinched(float frequency)
    {
        const int cycles = 10;
        const int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * cycles / frequency);
        var samples = new float[sampleCount];

        const int harmonics = 20;

        // Formant boost range (approx. 1200–2500 Hz)
        // At Bb1, this is roughly harmonics 5–7
        const int formantMin = 5;
        const int formantMax = 7;

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var basePhase = 2 * Mathf.PI * frequency * t;

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

        var _clip = AudioClip.Create("BuglePinchedClip", sampleCount, 1, sampleRate, false);
        _clip.SetData(samples, 0);
        return _clip;
    }
}