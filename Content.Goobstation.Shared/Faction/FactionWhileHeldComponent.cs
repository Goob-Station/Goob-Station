using Content.Shared.NPC.Prototypes;
using Content.Shared.StatusIcon;
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

    /// <summary>
    /// Optional faction icon shown on the holder while this is held.
    /// </summary>
    [DataField]
    public ProtoId<FactionIconPrototype>? IconToAdd;

    /// <summary>
    /// Whether this item added the icon, so we only remove an icon we put there.
    /// </summary>
    public bool IconAdded;
}
