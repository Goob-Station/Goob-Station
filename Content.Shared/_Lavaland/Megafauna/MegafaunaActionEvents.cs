using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna;

/// <summary>
/// Spawns an entity at user's coordinates.
/// </summary>
public sealed partial class MegafaunaInstantSpawnEntityAction : InstantActionEvent
{
    [DataField]
    public EntProtoId Id;

    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public bool ParentToUser;
}

/// <summary>
/// Spawns an entity on selected coordinates.
/// </summary>
public sealed partial class MegafaunaWorldSpawnEntityAction : WorldTargetActionEvent
{
    [DataField]
    public EntProtoId Id;

    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public bool ParentToUser;

    [DataField]
    public bool ParentToTarget;
}

/// <summary>
/// Teleports the boss on selected coordinates.
/// </summary>
public sealed partial class MegafaunaGrantComponentsAction : InstantActionEvent
{
    [DataField]
    public EntProtoId Id;

    [DataField]
    public ComponentRegistry? Components;

    [DataField]
    public bool ParentToUser;

    [DataField]
    public bool ParentToTarget;
}
