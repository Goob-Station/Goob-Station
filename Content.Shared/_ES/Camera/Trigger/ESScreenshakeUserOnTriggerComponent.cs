using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared._ES.Camera.Trigger;

/// <summary>
/// Screenshakes the user on trigger.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ESScreenshakeUserOnTriggerComponent : BaseXOnTriggerComponent
{
    [DataField]
    public ESScreenshakeParameters? Translation;

    [DataField]
    public ESScreenshakeParameters? Rotation;
}
