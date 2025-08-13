using System;
using UnityEngine;

namespace BugleMaestro.Helpers;

public class ClipHelper
{
    // Reference for note frequencies: https://www.liutaiomottola.com/formulae/freqtab.htm
    public static AudioClip RandomClip(ScaleEnum scaleNote)
    {
        var random = UnityEngine.Random.Range(0, 3);
        FrequencyAttribute frequencyAttr = ScaleHelper.GetAttributeOfScaleNote<FrequencyAttribute>(scaleNote);
        float frequency = frequencyAttr.Frequency;

        //Plugin.Log.LogInfo($"{Plugin.LOG_PREFIX}: Generating bugle clip at frequency {frequency}Hz");

        return CreatePeakBugle(frequency);
    }

    /*
    public static AudioClip Midi(float frequency)
    {
        var volume = 0.35f;
        var durationSeconds = 10; // todo
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            // Generate sine wave: sin(2pi * freq * time)
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate) * volume;
        }

        AudioClip clip = AudioClip.Create(
            name: $"Tone_{frequency}Hz",
            lengthSamples: sampleCount,
            channels: 1, // mono
            frequency: sampleRate,
            stream: false
        );

        clip.SetData(samples, 0);
        return clip;
    }*/


    public static AudioClip Midi(float frequency)
    {
        var volume = 0.35f;


        var cycles = 10; // durationSeconds
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * cycles);
        float[] samples = new float[sampleCount];

        // generate for each count
        for (int i = 0; i < sampleCount; i++)
        {
            // Generate sine wave: sin(2pi * freq * time)
            var time = i / (float)sampleRate;
            var basePhase = Mathf.Sin(2 * Mathf.PI * frequency * time * volume);

            samples[i] = basePhase;
        }

        for (var i = 0; i < sampleCount; i++)
        {
            var t = i / (float)sampleRate;
            var basePhase = 2 * Mathf.PI * frequency * t;

            var value = 0f;


            // Mild saturation to simulate horn compression (adds buzz edge)
            value += 0.15f * Mathf.Pow(value, 3f);

            // Slight breath noise
            var breath = 0.005f * (UnityEngine.Random.value - 0.5f);
            samples[i] = value * 0.3f + breath;
        }

        AudioClip clip = AudioClip.Create(
            name: $"Tone_{frequency}Hz",
            lengthSamples: sampleCount,
            channels: 1, // mono
            frequency: sampleRate,
            stream: false
        );

        clip.SetData(samples, 0);
        return clip;
    }

    public static AudioClip CreatePeakBugle(float baseFrequency, float durationSeconds = 3f, float volume = 0.8f)
    {
        int sampleRate = 48000;
        int sampleCount = Mathf.CeilToInt(sampleRate * durationSeconds);
        float[] samples = new float[sampleCount];
        System.Random rand = new System.Random();

        // Envelope timings (fast attack, natural decay, no sustain)
        float attackTime = 0.02f;
        float decayTime = durationSeconds * 0.85f; // most of the note is decay
        float releaseTime = durationSeconds * 0.13f;

        // Harmonic amplitude profile (from analysis)
        float[] harmonicMultipliers = { 1.0f, 2.11f, 2.22f, 3.0f };
        float[] harmonicAmplitudes = { 1.0f, 0.55f, 0.4f, 0.25f };

        // Slight detune factor for brass beating
        float detuneFactor = 0.015f; // 1.5% detune

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;

            // Envelope
            float env;
            if (t < attackTime) // Attack
                env = t / attackTime;
            else if (t < attackTime + decayTime) // Decay
                env = Mathf.Lerp(1f, 0.3f, (t - attackTime) / decayTime);
            else // Release
                env = Mathf.Lerp(0.3f, 0f, (t - attackTime - decayTime) / releaseTime);

            // Main tone: sum of harmonics + detuned beating
            float sampleVal = 0f;
            for (int h = 0; h < harmonicMultipliers.Length; h++)
            {
                float freq = baseFrequency * harmonicMultipliers[h];
                float harmonic = Mathf.Sin(2 * Mathf.PI * freq * t);
                float harmonicDetuned = Mathf.Sin(2 * Mathf.PI * freq * (1f + detuneFactor) * t);
                sampleVal += (harmonic + harmonicDetuned) * 0.5f * harmonicAmplitudes[h];
            }

            // Breath noise at start
            float noise = 0f;
            if (t < 0.12f) // 120 ms burst
                noise = ((float)rand.NextDouble() * 2f - 1f) * (0.05f * (1f - t / 0.12f));

            // Combine tone + noise
            float val = (sampleVal + noise) * env;

            // Soft clip (tanh-like) for brass warmth
            val = (float)Math.Tanh(val * 1.5f);

            samples[i] = val * volume;
        }

        AudioClip clip = AudioClip.Create($"PeakBugle_{baseFrequency}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
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