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
    public float staminaDamage;

    [DataField]
    public int effectTime;
}

[RegisterComponent]
public sealed partial class KravMagaComponent : GrabStagesOverrideComponent
{
    [DataField]
    public KravMagaMoves? SelectedMove;

    [DataField]
    public KravMagaActionComponent? SelectedMoveComp;

    public readonly List<ProtoId<EntityPrototype>> BaseKravMagaMoves = new()
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

[RegisterComponent,NetworkedComponent]
public sealed partial class KravMagaSilencedComponent : Component
{
    [DataField]
    public TimeSpan SilencedTime = TimeSpan.Zero;
}

[RegisterComponent,NetworkedComponent]
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

