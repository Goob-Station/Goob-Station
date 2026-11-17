using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.NPC.Components;

/// <summary>
/// While present, these factions are used instead of the entity's real factions.
/// Goob file btw
/// </summary>
[RegisterComponent]
public sealed partial class FactionOverrideComponent : Component
{
    [DataField]
    public HashSet<ProtoId<NpcFactionPrototype>> Factions = new();
}
