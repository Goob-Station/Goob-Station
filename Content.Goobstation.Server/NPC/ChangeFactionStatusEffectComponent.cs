using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.NPC;

[RegisterComponent]
public sealed partial class ChangeFactionStatusEffectComponent : Component
{
    [DataField] public ProtoId<NpcFactionPrototype>? NewFaction;
    [ViewVariables(VVAccess.ReadOnly)] public HashSet<ProtoId<NpcFactionPrototype>> OldFactions;
}
