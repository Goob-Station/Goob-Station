using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class KravMagaActionComponent : Component
{
    [DataField]
    public KravMagaMoves Configuration;

    [DataField]
    public string Name;

    [DataField]
    public float StaminaDamage;

    [DataField]
    public int EffectTime;
}

[RegisterComponent]
public sealed partial class KravMagaComponent : GrabStagesOverrideComponent
{
    [DataField]
    public KravMagaMoves? SelectedMove;

    [DataField]
    public KravMagaActionComponent? SelectedMoveComp;

    public readonly List<EntProtoId> BaseKravMagaMoves = new()
    {
        "ActionLegSweep",
        "ActionNeckChop",
        "ActionLungPunch",
    };

    public readonly List<EntityUid> KravMagaMoveEntities = new()
    {
    };

    [DataField]
    public int BaseDamage = 5;

    [DataField]
    public int DownedDamageModifier = 2;
}
/// <summary>
/// Tracks when an entity is silenced through Krav Maga techniques.
/// Prevents the affected entity from using voice-activated abilities or speaking.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KravMagaSilencedComponent : Component
{
    [DataField]
    public TimeSpan SilencedTime = TimeSpan.Zero;
}

/// <summary>
/// Tracks when an entity's breathing is blocked through Krav Maga techniques.
/// May cause suffocation damage over time when integrated with respiration systems.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KravMagaBlockedBreathingComponent : Component
{
    [DataField]
    public TimeSpan BlockedTime = TimeSpan.Zero;
}

public enum KravMagaMoves
{
    LegSweep,
    NeckChop,
    LungPunch,
}
