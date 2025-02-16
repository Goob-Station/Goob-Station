using Content.Shared.Light.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Clothing;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HandheldLightItemSlotComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? EntityActionReference;
    // The HandHeldLight entity that is currently attached to the slot if any.
    [DataField, AutoNetworkedField]
    public EntityUid? AttachedLight;
    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;
    [DataField, AutoNetworkedField]
    public string SlotName = "light";
}

[Serializable, NetSerializable]
public sealed class HandheldLightItemSlotComponentState : ComponentState
{
    // If null no light is attached.
    public LightState? State;

    public HandheldLightItemSlotComponentState(LightState? lightState)
    {
        State = lightState;
    }
}
[Serializable, NetSerializable]
public sealed class LightState
{
    public bool Activated;
    public HandheldLightPowerStates PowerState;
}
