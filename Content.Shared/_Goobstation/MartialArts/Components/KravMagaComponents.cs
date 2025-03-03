using Content.Shared.Actions;
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
}

[RegisterComponent]
public sealed partial class KravMagaComponent : Component
{
    [DataField]
    public KravMagaMoves? SelectedMove;

    public readonly List<ProtoId<EntityPrototype>> BaseKravMagaMoves = new()
    {
        "ActionLegSweep",
    };

    public readonly List<EntityUid> KravMagaMoveEntities = new()
    {
    };
}

public enum KravMagaMoves
{
    LegSweep,
    NeckChop,
    LungPunch,
}

public sealed partial class KravMagaActionEvent : InstantActionEvent
{
}
