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

public enum ScaleEnum
{ // C1, C2, C3, etc
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.C)]
    C1 = 1,
    CSharp1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.D)]
    D1,
    EFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.E)]
    E1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.F)]
    F1,
    FSharp1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.G)]
    G1,
    AFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.A)]
    A1,
    BFlat1,
    [Octave(OctaveEnum.Lowest)]
    [NamedNote(RawNoteInputEnum.B)]
    B1,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.C)]
    C2,
    CSharp2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.D)]
    D2,
    EFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.E)]
    E2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.F)]
    F2,
    FSharp2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.G)]
    G2,
    AFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.A)]
    A2,
    BFlat2,
    [Octave(OctaveEnum.Neutral)]
    [NamedNote(RawNoteInputEnum.B)]
    B2,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.C)]
    C3,
    CSharp3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.D)]
    D3,
    EFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.E)]
    E3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.F)]
    F3,
    FSharp3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.G)]
    G3,
    AFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.A)]
    A3,
    BFlat3,
    [Octave(OctaveEnum.Highest)]
    [NamedNote(RawNoteInputEnum.B)]
    B3
}