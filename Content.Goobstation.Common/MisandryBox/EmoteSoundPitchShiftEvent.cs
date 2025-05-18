namespace Content.Goobstation.Common.MisandryBox;

/// <summary>
/// Used by <see cref="CatEmoteSpamCountermeasureSystem"/> to pitch emote sounds as it nears to a smite
/// </summary>
/// <param name="pitch">additive pitch to the sound</param>
[ByRefEvent]
public struct EmoteSoundPitchShiftEvent(float pitch = 0)
{
    public float Pitch { get; set; } = pitch;
}
