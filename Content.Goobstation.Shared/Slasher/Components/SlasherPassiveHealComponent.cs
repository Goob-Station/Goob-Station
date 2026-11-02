using Content.Shared._Shitmed.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Heals slashers passively while they're corporeal. The amount healed scales with the fear meter.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SlasherPassiveHealComponent : Component
{
    [DataField]
    public float MinHealPerSecond = 0.2f;

    [DataField]
    public float MaxHealPerSecond = 4f;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField]
    public float MinBleedReductionPerSecond = 0.1f;

    [DataField]
    public float MaxBleedReductionPerSecond = 1f;

    [DataField]
    public float MinBloodRestoredPerSecond = 0.5f;

    [DataField]
    public float MaxBloodRestoredPerSecond = 3f;

    [DataField]
    public SplitDamageBehavior SplitBehavior = SplitDamageBehavior.Split;

    [ViewVariables, AutoNetworkedField, AutoPausedField]
    public TimeSpan NextHeal;
}
