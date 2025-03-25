using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.Silo;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SiloUtilizerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Silo;
}
