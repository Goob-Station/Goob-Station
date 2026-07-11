// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.BloodCult.Components;
using Content.Server.Body.Components;
using Content.Server.Examine;
using Content.Server.Popups;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Atmos.Components;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Trigger;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class BloodBoilRuneSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ExamineSystem _examine = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodBoilRuneComponent, TriggerEvent>(OnTriggered);
        SubscribeLocalEvent<BloodBoilProjectileComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnTriggered(Entity<BloodBoilRuneComponent> rune, ref TriggerEvent args)
    {
        if (args.Handled || args.User is not { } user || !HasComp<BloodCultistComponent>(user))
            return;

        var invokers = GetInvokers(rune);
        if (invokers.Count < rune.Comp.RequiredInvokers)
        {
            _popup.PopupEntity(
                Loc.GetString("cult-blood-boil-not-enough-invokers", ("required", rune.Comp.RequiredInvokers)),
                rune,
                user);
            return;
        }

        var targets = GetTargets(rune);
        if (targets.Count == 0)
        {
            _popup.PopupEntity(Loc.GetString("cult-blood-boil-no-targets"), rune, user);
            return;
        }

        foreach (var invoker in invokers)
            _damageable.TryChangeDamage(invoker, new DamageSpecifier(rune.Comp.InvocationDamage), true, origin: rune);

        var projectileCount = Math.Min(targets.Count, rune.Comp.ProjectileCount);
        for (var i = 0; i < projectileCount; i++)
        {
            var target = _random.PickAndTake(targets);
            if (TryComp<FlammableComponent>(target, out var flammable))
            {
                _flammable.AdjustFireStacks(target, rune.Comp.FireStacksPerProjectile, flammable);
                _flammable.Ignite(target, rune, flammable);
            }

            Shoot(rune, target);
        }

        _audio.PlayPvs(rune.Comp.ActivationSound, rune, AudioParams.Default.WithMaxDistance(2f));
        args.Handled = true;
    }

    private List<EntityUid> GetInvokers(Entity<BloodBoilRuneComponent> rune)
    {
        var invokers = new List<EntityUid>();
        var nearby = _lookup.GetEntitiesInRange(Transform(rune).Coordinates, rune.Comp.InvokerRange);

        foreach (var entity in nearby)
        {
            if (!_mobState.IsAlive(entity) ||
                !HasComp<BloodCultistComponent>(entity) && !HasComp<BloodCultConstructComponent>(entity))
                continue;

            invokers.Add(entity);
        }

        return invokers;
    }

    private List<EntityUid> GetTargets(Entity<BloodBoilRuneComponent> rune)
    {
        var targets = new List<EntityUid>();
        var nearby = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(
            Transform(rune).Coordinates,
            rune.Comp.TargetRange);

        foreach (var target in nearby)
        {
            if (HasComp<BloodCultistComponent>(target) ||
                HasComp<BloodCultConstructComponent>(target) ||
                !HasComp<BloodstreamComponent>(target) ||
                !_mobState.IsAlive(target) ||
                !_examine.InRangeUnOccluded(rune, target, rune.Comp.TargetRange))
                continue;

            targets.Add(target);
        }

        return targets;
    }

    private void Shoot(Entity<BloodBoilRuneComponent> rune, EntityUid target)
    {
        var runePosition = _transform.GetMapCoordinates(rune);
        var targetPosition = _transform.GetMapCoordinates(target);
        var projectile = Spawn(rune.Comp.ProjectilePrototype, runePosition);

        if (!HasComp<ProjectileComponent>(projectile))
        {
            QueueDel(projectile);
            return;
        }

        EnsureComp<BloodBoilProjectileComponent>(projectile).Target = target;
        var direction = targetPosition.Position - runePosition.Position;
        _gun.ShootProjectile(projectile, direction, Vector2.Zero, rune, rune, rune.Comp.ProjectileSpeed);
    }

    private void OnPreventCollide(Entity<BloodBoilProjectileComponent> projectile, ref PreventCollideEvent args)
    {
        if (args.OtherEntity != projectile.Comp.Target)
            args.Cancelled = true;
    }
}
