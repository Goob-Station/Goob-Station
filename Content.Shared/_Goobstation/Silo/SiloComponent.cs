using Content.Shared.Materials;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Silo;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SiloComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public Dictionary<ProtoId<MaterialPrototype>, int> Storage { get; set; } = new();
}
