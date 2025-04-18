using Robust.Shared.Audio;

namespace Content.Goobstation.Common.Silicons.Components;

/// <summary>
/// Actively provides a random lawset to some entities
/// If the timer ticks down gives it's reward to a research server
/// </summary>
[RegisterComponent]
public sealed partial class ActiveExperimentalLawProviderComponent : Component
{
    [DataField]
    public string OldSiliconLawsetId = string.Empty;

    [DataField]
    public float Timer = 120.0f;

    [DataField]
    public int RewardPoints = 5000;

    [DataField]
    public SoundSpecifier? LawRewardSound = new SoundPathSpecifier("/Audio/Misc/cryo_warning.ogg");
}
