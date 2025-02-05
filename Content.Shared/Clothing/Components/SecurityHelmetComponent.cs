
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing;

[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SecurityHelmetComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? SecliteEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;
}
