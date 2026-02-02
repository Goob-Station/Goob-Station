using Robust.Shared.Audio;
using Robust.Shared.Serialization;

// this exists in shared because i need it for y*ml

namespace Content.Goobstation.Shared.Cult;

[Serializable, NetSerializable]
public struct CultTierData
{
    [DataField] public LocId Announcement;
    [DataField] public SoundSpecifier Sound;

    public CultTierData(string announcement, SoundSpecifier gainSound)
    {
        Announcement = announcement;
        Sound = gainSound;
    }
}

[Serializable, NetSerializable]
public enum BloodCultTier : int
{
    /// <summary>
    ///     No visual changes
    /// </summary>
    None = 0,

    /// <summary>
    ///     Grr kitten... *eyes glow red* *and valid*
    /// </summary>
    Eyes = 1,

    /// <summary>
    ///     VERY VALID halos start appearing
    /// </summary>
    Halos = 2
}

[Serializable, NetSerializable]
public enum BloodCultWinType : byte
{
    /// <summary>
    ///     NarSie got summoned.
    /// </summary>
    CultMajor,

    /// <summary>
    ///    Round ended, but all cult winning conditions were met.
    /// </summary>
    CultMinor,

    /// <summary>
    ///     Neutral. Cult didn't reach halos and/or escaped with the crew.
    /// </summary>
    Neutral,

    /// <summary>
    ///     All cultists were marooned and halos did not appear.
    /// </summary>
    CrewMinor,

    /// <summary>
    ///     All cultists were killed/deconverted.
    /// </summary>
    CrewMajor
}
