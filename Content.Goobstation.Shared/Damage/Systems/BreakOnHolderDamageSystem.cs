using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Damage;

public sealed class BreakOnHolderDamageSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityQuery<BreakOnHolderDamageComponent> _breakQuery;

    public override void Initialize()
    {
        base.Initialize();

        _breakQuery = GetEntityQuery<BreakOnHolderDamageComponent>();

        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<DamageableComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        if (args.Origin is null || args.Origin == ent.Owner)
            return;

        if (!HasComp<HandsComponent>(ent) && !HasComp<InventoryComponent>(ent))
            return;

        foreach (var item in _inventory.GetHandOrInventoryEntities(ent.Owner))
        {
            if (!_breakQuery.TryComp(item, out var comp))
            {
                continue;
            }

            Break((item, comp), ent.Owner);
        }
    }

    private void Break(Entity<BreakOnHolderDamageComponent> ent, EntityUid holder)
    {
        var coords = Transform(ent).Coordinates;

        _audio.PlayPredicted(ent.Comp.BreakSound, coords, holder);

        if (ent.Comp.SpawnOnBreak is not null)
        {
            PredictedSpawnAtPosition(ent.Comp.SpawnOnBreak, coords);
        }

        PredictedQueueDel(ent);
    }
}
