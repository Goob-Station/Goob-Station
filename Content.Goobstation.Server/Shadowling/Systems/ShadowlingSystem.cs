using Content.Goobstation.Shared.Flashbang;
using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Systems;
using Content.Server.Objectives.Systems;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles the Shadowling's System
/// </summary>
public sealed class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAscendEvent>(OnAscend);
        SubscribeLocalEvent<ShadowlingComponent, SelfBeforeGunShotEvent>(BeforeGunShot);
        SubscribeLocalEvent<ShadowlingComponent, GetFlashbangedEvent>(OnFlashBanged);
    }

    private void OnAscend(ShadowlingAscendEvent args)
    {
        if (TryComp<ShadowlingComponent>(args.ShadowlingAscended, out var comp))
            _codeCondition.SetCompleted(args.ShadowlingAscended, comp.ObjectiveAscend);
    }

    private void BeforeGunShot(Entity<ShadowlingComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        // Slings cant shoot guns
        if (args.Gun.Comp.ClumsyProof)
            return;

        if (!_random.Prob(0.5f))
            return;

        _damageable.TryChangeDamage(ent, ent.Comp.GunShootFailDamage, origin: ent);

        _stun.TryParalyze(ent, ent.Comp.GunShootFailStunTime, false);

        args.Cancel();
    }

    private void OnFlashBanged(EntityUid uid, ShadowlingComponent component, GetFlashbangedEvent args)
    {
        // Shadowling get damaged from flashbangs
        if (!TryComp<DamageableComponent>(uid, out var damageableComp))
            return;

        _damageable.TryChangeDamage(uid, component.HeatDamage, damageable: damageableComp);
    }

    protected override void StartHatchingProgress(Entity<ShadowlingComponent> ent)
    {
        var (uid, comp) = ent;

        comp.IsHatching = true;

        // Drop all items
        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            foreach (var slot in inv.Slots)
            {
                _inventorySystem.DropSlotContents(uid, slot.Name, inv);
            }
        }

        var egg = SpawnAtPosition(comp.Egg, Transform(uid).Coordinates);
        if (TryComp<HatchingEggComponent>(egg, out var eggComp)
            && TryComp<EntityStorageComponent>(egg, out var eggStorage))
        {
            eggComp.ShadowlingInside = uid;
            _entityStorage.Insert(uid, egg, eggStorage);
        }

        // It should be noted that Shadowling shouldn't be able to take damage during this process.
    }
}
