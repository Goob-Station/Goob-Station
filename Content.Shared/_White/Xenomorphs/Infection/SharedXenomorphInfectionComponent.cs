using Content.Shared._White.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Infection;

[NetworkedComponent]
public abstract partial class SharedXenomorphInfectionComponent : Component
{
    /// <summary>
    /// A set of prototype IDs for status icons representing different growth stages of the infection.
    /// </summary>
    [DataField]
    public Dictionary<int, ProtoId<InfectionIconPrototype>> InfectedIcons = new();

    /// <summary>
    /// Current stage of infection development.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int GrowthStage;
}
