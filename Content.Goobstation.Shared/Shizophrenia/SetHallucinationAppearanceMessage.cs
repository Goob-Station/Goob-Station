using Robust.Shared.Audio;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Shizophrenia;

[Serializable, NetSerializable]
public sealed partial class SetHallucinationAppearanceMessage : EntityEventArgs
{
    public HallucinationAppearanceData Appearance = new();

    public SetHallucinationAppearanceMessage(HallucinationAppearanceData appearance)
    {
        Appearance = appearance;
    }
}

/// <summary>
/// Data container for fake appearance applied to entities while player is hallucinating
/// </summary>
[DataDefinition]
[Serializable, NetSerializable]
public sealed partial class HallucinationAppearanceData
{
    /// <summary>
    /// Prototypes which sprites could be used
    /// </summary>
    [DataField]
    public List<string> Prototypes;

    /// <summary>
    /// Rsi states
    /// </summary>
    [DataField]
    public List<string> States = new();

    /// <summary>
    /// Sound played clientside
    /// </summary>
    [DataField]
    public SoundSpecifier? Sound;
}
