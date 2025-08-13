using System;
using System.Linq;
using System.Reflection;

namespace BugleMaestro.Helpers;

public class ScaleHelper
{
    public readonly static ScaleEnum DEFAULT_NOTE = ScaleEnum.C3;
    public const float FundamentalFrequency = 130.813f; // C3
    public readonly static RawNoteInputEnum DEFAULT_RAW_NOTE = RawNoteInputEnum.C;
    public readonly static OctaveEnum DEFAULT_OCTAVE = OctaveEnum.Neutral;
    public readonly static SemitoneModifierEnum DEFAULT_SEMITONE_MODIFIER = SemitoneModifierEnum.Natural;
    
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

public class UIDisplayNameAttribute : Attribute
{
    public string UIDisplayName { get; private set; }

    public UIDisplayNameAttribute(string name)
    {
        UIDisplayName = name;
    }
}

// Reference frequencies from https://www.liutaiomottola.com/formulae/freqtab.htm
public enum ScaleEnum
{ // C2, C3, C4, etc
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(65.406f)]
    [UIDisplayNameAttribute("C2")]
    C2 = 1,
    [Frequency(69.296f)]
    [UIDisplayNameAttribute("C♯2")]
    CSharp2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(73.416f)]
    [UIDisplayNameAttribute("D2")]
    D2,
    [Frequency(77.782f)]
    [UIDisplayNameAttribute("E♭2")]
    EFlat2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(82.406f)]
    [UIDisplayNameAttribute("E2")]
    E2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(87.308f)]
    [UIDisplayNameAttribute("F2")]
    F2,
    [Frequency(92.498f)]
    [UIDisplayNameAttribute("F♯2")]
    FSharp2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(97.998f)]
    [UIDisplayNameAttribute("G2")]
    G2,
    [Frequency(103.826f)]
    [UIDisplayNameAttribute("A♭2")]
    AFlat2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(110f)]
    [UIDisplayNameAttribute("A2")]
    A2,
    [Frequency(116.541f)]
    [UIDisplayNameAttribute("B♭2")]
    BFlat2,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(123.471f)]
    [UIDisplayNameAttribute("B2")]
    B2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(ScaleHelper.FundamentalFrequency * 2)]
    [UIDisplayNameAttribute("C3")]
    C3,
    [Frequency(138.592f)]
    [UIDisplayNameAttribute("C♯3")]
    CSharp3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(146.832f)]
    [UIDisplayNameAttribute("D3")]
    D3,
    [Frequency(155.563f)]
    [UIDisplayNameAttribute("E♭3")]
    EFlat3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(164.814f)]
    [UIDisplayNameAttribute("E3")]
    E3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(174.614f)]
    [UIDisplayNameAttribute("F3")]
    F3,
    [Frequency(184.997f)]
    [UIDisplayNameAttribute("F♯3")]
    FSharp3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(195.998f)]
    [UIDisplayNameAttribute("G3")]
    G3,
    [Frequency(207.652f)]
    [UIDisplayNameAttribute("A♭3")]
    AFlat3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(220f)]
    [UIDisplayNameAttribute("A3")]
    A3,
    [Frequency(233.082f)]
    [UIDisplayNameAttribute("B♭3")]
    BFlat3,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(246.942f)]
    [UIDisplayNameAttribute("B3")]
    B3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.C)]
    [Frequency(261.626f)]
    [UIDisplayNameAttribute("C4")]
    C4,
    [Frequency(277.182f)]
    [UIDisplayNameAttribute("C♯4")]
    CSharp4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.D)]
    [Frequency(293.664f)]
    [UIDisplayNameAttribute("D4")]
    D4,
    [Frequency(311.126f)]
    [UIDisplayNameAttribute("E♭4")]
    EFlat4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.E)]
    [Frequency(329.628f)]
    [UIDisplayNameAttribute("E4")]
    E4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.F)]
    [Frequency(349.228f)]
    [UIDisplayNameAttribute("F4")]
    F4,
    [Frequency(369.994f)]
    [UIDisplayNameAttribute("F♯4")]
    FSharp4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.G)]
    [Frequency(391.996f)]
    [UIDisplayNameAttribute("G4")]
    G4,
    [Frequency(415.304f)]
    [UIDisplayNameAttribute("A♭4")]
    AFlat4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.A)]
    [Frequency(440f)]
    [UIDisplayNameAttribute("A4")]
    A4,
    [Frequency(466.164f)]
    [UIDisplayNameAttribute("B♭4")]
    BFlat4,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.B)]
    [Frequency(493.884f)]
    [UIDisplayNameAttribute("B4")]
    B4
}