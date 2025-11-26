using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Component that allows an augment to have an action.
/// If the action uses <c>ToggleActionEvent</c> then it will try to toggle the augment, if it has power etc.
/// The icon being toggled is then synced with the augment's activated status.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AugmentActionComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Action;

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;
}

/// <summary>
/// A generic event for augments which doesn't need any targeting.
/// Does nothing on its own, it needs other systems to handle it.
/// </summary>
public sealed partial class AugmentActionEvent : InstantActionEvent;
