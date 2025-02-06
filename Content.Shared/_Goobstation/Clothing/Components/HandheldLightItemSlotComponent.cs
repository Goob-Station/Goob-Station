
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HandheldLightItemSlotComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LightEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;
}
