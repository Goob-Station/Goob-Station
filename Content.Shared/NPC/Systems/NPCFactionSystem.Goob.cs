using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.NPC.Systems;

public sealed partial class NpcFactionSystem
{
    private static readonly IReadOnlySet<ProtoId<NpcFactionPrototype>> NoFactions = new HashSet<ProtoId<NpcFactionPrototype>>();

    [SubscribeLocalEvent]
    private void OnGetFactionOverride(Entity<FactionOverrideComponent> ent, ref GetFactionOverrideEvent args)
    {
        args.Factions = ent.Comp.Factions;
    }

    /// <summary>
    /// Returns the factions this entity should be treated as having, with any override applied.
    /// </summary>
    public IReadOnlySet<ProtoId<NpcFactionPrototype>> GetFactions(Entity<NpcFactionMemberComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false)) return NoFactions;

        var ev = new GetFactionOverrideEvent();
        RaiseLocalEvent(ent, ref ev);

        return ev.Factions ?? ent.Comp.Factions;
    }
}

/// <summary>
/// Raised on an entity when its factions are read.
/// </summary>
[ByRefEvent]
public record struct GetFactionOverrideEvent(HashSet<ProtoId<NpcFactionPrototype>>? Factions = null);
