using Content.Shared.NPC.Prototypes;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Faction;

/// <summary>
/// While held, makes holder a specific faction instead of the faction they belong to.
/// </summary>
[RegisterComponent]
public sealed partial class FactionWhileHeldComponent : Component
{
    [DataField(required: true)]
    public ProtoId<NpcFactionPrototype> Faction;

    /// <summary>
    /// Optional faction icon shown on the holder while this is held.
    /// </summary>
    [DataField]
    public ProtoId<FactionIconPrototype>? IconToAdd;

    /// <summary>
    /// Whether this item added the icon, so we only remove an icon we put there.
    /// </summary>
    public bool IconAdded;

    /// <summary>
    /// Whether this item applied the faction override, so we only remove an override we put there.
    /// </summary>
    public bool Applied;
}
