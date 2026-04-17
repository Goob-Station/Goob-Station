using Content.Goobstation.Shared.Terror.Components;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.Systems;
using Content.Shared.Inventory.Events;
using Content.Shared.RatKing;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Goobstation.Server.Terror.Systems;

public sealed class SpiderPetCrownSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpiderPetCrownComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<SpiderPetCrownComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<SpiderPetCrownComponent, ComponentShutdown>(OnCrownShutdown);
        SubscribeLocalEvent<SpiderPetComponent, ComponentShutdown>(OnPetShutdown);
    }

    private void OnEquipped(EntityUid uid, SpiderPetCrownComponent comp, GotEquippedEvent args)
    {
        if (args.Slot != "head")
            return;

        var pet = Spawn(comp.PetPrototype, Transform(args.Equipee).Coordinates);
        var petComp = EnsureComp<SpiderPetComponent>(pet);
        petComp.MasterUid = args.Equipee;
        comp.Pet = pet;

        // Tell the HTN to always follow the wearer
        _npc.SetBlackboard(pet, NPCBlackboard.FollowTarget, new EntityCoordinates(args.Equipee, Vector2.Zero));
        Dirty(pet, petComp);
    }

    private void OnUnequipped(EntityUid uid, SpiderPetCrownComponent comp, GotUnequippedEvent args)
    {
        if (args.Slot != "head")
            return;

        if (comp.Pet is { } pet && !TerminatingOrDeleted(pet))
            QueueDel(pet);

        comp.Pet = null;
    }

    private void OnCrownShutdown(EntityUid uid, SpiderPetCrownComponent comp, ComponentShutdown args)
    {
        if (comp.Pet is { } pet && !TerminatingOrDeleted(pet))
            QueueDel(pet);
    }

    private void OnPetShutdown(EntityUid uid, SpiderPetComponent comp, ComponentShutdown args)
    {
        var enumerator = EntityQueryEnumerator<SpiderPetCrownComponent>();
        while (enumerator.MoveNext(out _, out var crown))
        {
            if (crown.Pet == uid)
            {
                crown.Pet = null;
                break;
            }
        }
    }
}
