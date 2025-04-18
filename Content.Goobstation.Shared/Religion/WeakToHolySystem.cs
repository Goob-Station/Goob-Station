// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Religion;

public sealed class WeakToHolySystem : EntitySystem
{
    public const string ContainerId = "Biological";
    public const string TransformedContainerId = "BiologicalMetaphysical";
    public const string PassiveDamageType = "Holy";

    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<WeakToHolyComponent, DamageUnholyEvent>(OnUnholyRoundstartDamage);

        SubscribeLocalEvent<InventoryComponent, DamageUnholyEvent>(OnUnholyItemDamage);

        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);

        SubscribeLocalEvent<NullrodComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnStartup(Entity<WeakToHolyComponent> ent, ref ComponentStartup args)
    {
        // Only change to "BiologicalMetaphysical" if the original damage container was "Biological"
        if (TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == ContainerId)
            _damageableSystem.ChangeDamageContainer(ent, TransformedContainerId);
    }

    // used for entities that should take holy damage always e.g ghouls
    private void OnUnholyRoundstartDamage(Entity<WeakToHolyComponent> ent, ref DamageUnholyEvent args)
    {
        if (!ent.Comp.RoundStart)
            return;

        args.Handled = true;
    }

    // used for entities that should only be weak to holy damage when wearing or holding unholy items
    private void OnUnholyItemDamage(Entity<InventoryComponent> ent, ref DamageUnholyEvent args)
    {
        foreach (var item in
                 _inventorySystem.GetHandOrInventoryEntities(args.Target, SlotFlags.All & ~SlotFlags.POCKET))
        {
            if (HasComp<UnholyItemComponent>(item))
            {
                args.Handled = true;
                return;
            }
        }
    }

    private void OnMeleeHit(Entity<NullrodComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.HolyDamage == null)
            return;

        foreach (var target in args.HitEntities)
        {
            var unholyEvent = new DamageUnholyEvent(target, ent.Comp.HolyDamage, args.User);
            RaiseLocalEvent(target, unholyEvent);

            if (unholyEvent.Handled)
                _damageableSystem.TryChangeDamage(unholyEvent.Target, unholyEvent.Damage, origin: unholyEvent.Origin);
        }
    }

    // Passively heal on runes
    private void OnCollide(Entity<HereticRitualRuneComponent> ent, ref StartCollideEvent args)
    {
        var entityDamageComp = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) &&
            entityDamageComp.Damage.DamageDict.TryGetValue(PassiveDamageType, out _))
            return;

        // Store the original DamageCap if it hasn't already been stored
        if (!_originalDamageCaps.ContainsKey(args.OtherEntity))
            _originalDamageCaps[args.OtherEntity] = entityDamageComp.DamageCap;

        entityDamageComp.Damage.DamageDict.TryAdd(PassiveDamageType, ent.Comp.RuneHealing);
        entityDamageComp.DamageCap = FixedPoint2.New(0);
        DirtyEntity(args.OtherEntity);
    }

    private void OnCollideEnd(Entity<HereticRitualRuneComponent> ent, ref EndCollideEvent args)
    {
        if (!TryComp<PassiveDamageComponent>(args.OtherEntity, out var heretic))
            return;

        // Restore the original DamageCap if it was stored
        if (_originalDamageCaps.TryGetValue(args.OtherEntity, out var originalCap))
        {
            heretic.DamageCap = originalCap;
            _originalDamageCaps.Remove(args.OtherEntity); // Clean up after restoring
        }

        heretic.Damage.DamageDict.Remove("Holy");
        DirtyEntity(args.OtherEntity);
    }
}
