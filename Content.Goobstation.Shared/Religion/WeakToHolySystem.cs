// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Religion;

public sealed partial class WeakToHolySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    private readonly Dictionary<EntityUid, FixedPoint2> _originalDamageCaps = new();

    public const string ContainerId = "Biological";
    public const string TransformedContainerId = "BiologicalMetaphysical";
    public const string PassiveDamageType = "Holy";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeakToHolyComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HereticRitualRuneComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<HereticRitualRuneComponent, EndCollideEvent>(OnCollideEnd);
    }

    private void OnStartup(Entity<WeakToHolyComponent> ent, ref ComponentStartup args)
    {
        // Only change to "BiologicalMetaphysical" if the original damage container was "Biological"
        if (TryComp<DamageableComponent>(ent, out var damageable) && damageable.DamageContainerID == ContainerId)
            _damageableSystem.ChangeDamageContainer(ent, TransformedContainerId);
    }

    // Passively heal
    private void OnCollide(Entity<HereticRitualRuneComponent> ent, ref StartCollideEvent args)
    {
        var entityDamageComp = EnsureComp<PassiveDamageComponent>(args.OtherEntity);

        if (!HasComp<WeakToHolyComponent>(args.OtherEntity) && entityDamageComp.Damage.DamageDict.TryGetValue(PassiveDamageType, out _))
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
