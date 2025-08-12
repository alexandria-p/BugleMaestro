using System;
using System.Linq;
using System.Reflection;

namespace BugleMaestro.Helpers;

public class ScaleHelper
{
    public readonly static ScaleEnum DEFAULT_NOTE = ScaleEnum.C2;
    public readonly static RawNoteInputEnum DEFAULT_RAW_NOTE = RawNoteInputEnum.C;
    public readonly static OctaveEnum DEFAULT_OCTAVE = OctaveEnum.Neutral;
    public readonly static SemitoneModifierEnum DEFAULT_SEMITONE_MODIFIER = SemitoneModifierEnum.Natural;
    public const float FundamentalFrequency = 65.406f; // C2

    public static ScaleEnum LOWEST_NOTE => Enum.GetValues(typeof(ScaleEnum)).Cast<ScaleEnum>().OrderBy(x => x).First();
    public static ScaleEnum HIGHEST_NOTE => Enum.GetValues(typeof(ScaleEnum)).Cast<ScaleEnum>().OrderBy(x => x).Last();

    public static ScaleEnum CalculateScaleNote(RawNoteInputEnum rawInput, OctaveEnum octaveInput, SemitoneModifierEnum semitoneInput)
    {
        // Get base note input:
        ScaleEnum noteAdjustedForOctave = GetScaleEnumFromAttributes(rawInput, octaveInput);

        // Check Semitone:
        if (semitoneInput == SemitoneModifierEnum.Natural)
        {
            return noteAdjustedForOctave; // no modifications
        }
        else if (semitoneInput == SemitoneModifierEnum.Flat)
        {
            // If already the lowest note, just return the lowest note;   
            if (noteAdjustedForOctave == LOWEST_NOTE)
            {
                return LOWEST_NOTE;
            }
            else
            {
                // Apply Semitone modifier:
                int modifiedNoteAsInteger = (int)noteAdjustedForOctave - 1;
                return (ScaleEnum)modifiedNoteAsInteger;
            }
        }
        else if (semitoneInput == SemitoneModifierEnum.Sharp)
        {
            // If already the highest note, just return the highest note;   
            if (noteAdjustedForOctave == HIGHEST_NOTE)
            {
                return HIGHEST_NOTE;
            }
            else
            {
                // Apply Semitone modifier:
                int modifiedNoteAsInteger = (int)noteAdjustedForOctave + 1;
                return (ScaleEnum)modifiedNoteAsInteger;
            }
        }
        else
        {
            // No match found
            Plugin.Log.LogError($"{Plugin.LOG_PREFIX}: Could not find scale enum for RawNoteInputEnum {rawInput} and Semitone Modifier {semitoneInput} and Octave {octaveInput}.");
            return DEFAULT_NOTE;
        }
    }

    public static ScaleEnum GetScaleEnumFromAttributes(RawNoteInputEnum note, OctaveEnum octave)
    {
        var type = typeof(ScaleEnum);

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var octaveAttr = field.GetCustomAttribute<OctaveAttribute>();
            var noteAttr = field.GetCustomAttribute<NamedNoteAttribute>();

            if (octaveAttr?.Octave == octave && noteAttr?.RawNote == note)
            {
                return (ScaleEnum)field.GetValue(null);
            }
        }

        // No match found
        Plugin.Log.LogError($"{Plugin.LOG_PREFIX}: Could not find scale enum for RawNoteInputEnum {note} and OctaveEnum {octave}.");
        return DEFAULT_NOTE;
    }

    public static TAttribute GetAttributeOfScaleNote<TAttribute>(ScaleEnum value) where TAttribute : Attribute
    {
        var enumType = value.GetType();
        var name = Enum.GetName(enumType, value);
        return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().Single(); // throws exception if not found
    }
}

public enum RawNoteInputEnum
{
    C = 0,
    D,
    E,
    F,
    G,
    A,
    B
}

public enum OctaveEnum
{
    Lowest,
    Neutral,
    Highest
}

public enum SemitoneModifierEnum
{
    Flat,
    Sharp,
    Natural
}

public class NamedNoteAttribute : Attribute
{
    public RawNoteInputEnum RawNote { get; private set; }

    public NamedNoteAttribute(RawNoteInputEnum note)
    {
        RawNote = note;
    }
}

public class OctaveAttribute : Attribute
{
    public OctaveEnum Octave { get; private set; }

    public OctaveAttribute(OctaveEnum oct)
    {
        Octave = oct;
    }
}

public class FrequencyAttribute : Attribute
{
    public float Frequency { get; private set; }

    public FrequencyAttribute(float freq)
    {
        Frequency = freq;
    }
}

// Reference for note frequencies: https://www.liutaiomottola.com/formulae/freqtab.htm
public enum ScaleEnum
{ // C1, C2, C3, etc
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(32.703f)]
    C1 = 1,
    [Frequency(34.648f)]
    CSharp1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(36.708f)]
    D1,
    [Frequency(38.891f)]
    EFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(41.203f)]
    E1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(43.654f)]
    F1,
    [Frequency(46.249f)]
    FSharp1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(48.999f)]
    G1,
    [Frequency(51.913f)]
    AFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(55f)]
    A1,
    [Frequency(58.27f)]
    BFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(61.735f)]
    B1,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(ScaleHelper.FundamentalFrequency)]
    C2,
    [Frequency(69.296f)]
    CSharp2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(73.416f)]
    D2,
    [Frequency(77.782f)]
    EFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(82.407f)]
    E2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(87.307f)]
    F2,
    [Frequency(92.499f)]
    FSharp2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(97.999f)]
    G2,
    [Frequency(103.826f)]
    AFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(110f)]
    A2,
    [Frequency(116.541f)]
    BFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(123.471f)]
    B2,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(130.813f)]
    C3,
    [Frequency(138.591f)]
    CSharp3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(146.832f)]
    D3,
    [Frequency(155.563f)]
    EFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(164.814f)]
    E3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(174.614f)]
    F3,
    [Frequency(184.997f)]
    FSharp3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(195.998f)]
    G3,
    [Frequency(207.652f)]
    AFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(220f)]
    A3,
    [Frequency(233.082f)]
    BFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(246.942f)]
    B3
}