using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.NPC;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangeFactionStatusEffectComponent : Component
{
    [DataField]
    public ProtoId<NpcFactionPrototype>? NewFaction;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public HashSet<ProtoId<NpcFactionPrototype>> OldFactions = [];
}
