using Content.Goobstation.Common.Mech;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;

namespace Content.Goobstation.Shared.Mech;

public sealed class SharedMechSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NpcFactionMemberComponent, MechInsertedEvent>(OnFactionPilotInserted);
    }

    private void OnFactionPilotInserted(Entity<NpcFactionMemberComponent> ent, ref MechInsertedEvent args)
    {
        _faction.ClearFactions((args.mechUid, null), false);
        _faction.AddFactions((args.mechUid, null), ent.Comp.Factions);
    }

    private void OnFactionPilotEjected(Entity<NpcFactionMemberComponent> ent, ref MechEjectedEvent args)
    {
        // as-is this will break for mechs that for whatever reason also have a faction
        // but mechs shouldn't generally have a faction so
        _faction.ClearFactions((args.mechUid, null));
    }
}
