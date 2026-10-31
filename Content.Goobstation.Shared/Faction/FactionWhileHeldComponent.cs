using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Faction;

/// <summary>
/// While held, the holder's factions are replaced by Faction. Their original factions are restored when it is dropped.
/// </summary>
[RegisterComponent]
public sealed partial class FactionWhileHeldComponent : Component
{
    [DataField(required: true)]
    public ProtoId<NpcFactionPrototype> Faction;

    /// <summary>
    /// The holder's original factions, set only while the swap is active.
    /// </summary>
    public HashSet<ProtoId<NpcFactionPrototype>>? Stored;
}
