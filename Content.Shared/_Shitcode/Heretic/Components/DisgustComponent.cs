using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

/// <summary>
/// Damages silicons on rust tiles, causes negative effects and vomiting to non-silicons over time
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DisgustComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentLevel = 5f;

    [DataField]
    public float PassiveReduction = 0.5f;

    [DataField]
    public float NegativeEffectProb = 0.05f;

    [DataField]
    public float BadNegativeEffectProb = 0.13f;

    [DataField]
    public float ModifierPerUpdate = 5f;

    [DataField]
    public TimeSpan NegativeTime = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan BadNegativeTime = TimeSpan.FromSeconds(6);

    [DataField]
    public TimeSpan VomitKnockdownTime = TimeSpan.FromSeconds(20);

    [DataField]
    public float SlowdownMultiplier = 0.5f;

    [DataField]
    public float NegativeThreshold = 25f;

    [DataField]
    public float VomitThreshold = 50f;

    [DataField]
    public float BadNegativeThreshold = 75f;
}
