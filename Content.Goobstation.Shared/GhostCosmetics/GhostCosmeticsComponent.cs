using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.GhostCosmetics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class GhostCosmeticsComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<GhostCosmeticPrototype>? Particles;

    [DataField, AutoNetworkedField]
    public ProtoId<GhostCosmeticPrototype>? Hat;

    [DataField, AutoNetworkedField]
    public ProtoId<GhostCosmeticPrototype>? Mask;
}
