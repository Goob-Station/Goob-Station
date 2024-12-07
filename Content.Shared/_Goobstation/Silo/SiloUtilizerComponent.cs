using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Silo;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SiloUtilizerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Silo;
}
