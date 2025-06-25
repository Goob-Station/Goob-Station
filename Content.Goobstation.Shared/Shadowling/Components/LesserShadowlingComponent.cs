using Content.Shared.Alert;
using Content.Shared.Polymorph;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling.Components;


/// <summary>
/// This is used for indicating that the user is Lesser Shadowling
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LesserShadowlingComponent : Component
{
    // todo: add new status icon for them? will consider it after everything's done

    public readonly string? ShadowWalkAction = "ActionShadowWalk";

    public EntityUid? ShadowWalkActionId;

    [DataField]
    public ProtoId<AlertPrototype> AlertProto = "ShadowlingLight";
}
