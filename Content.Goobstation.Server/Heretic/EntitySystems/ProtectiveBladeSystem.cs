// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Damage;
using Content.Shared.Follower;
using Content.Shared.Follower.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Server.Heretic.Abilities;
using Content.Goobstation.Server.Heretic.Components;
using Content.Server.Buckle.Systems;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.Heretic.EntitySystems;

public sealed partial class ProtectiveBladeSystem : EntitySystem
{
    [Dependency] private readonly FollowerSystem _follow = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ReflectSystem _reflect = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public static readonly EntProtoId BladePrototype = "HereticProtectiveBlade";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProtectiveBladeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ProtectiveBladeComponent, InteractHandEvent>(OnInteract,
            after: [typeof(BuckleSystem)]);

        SubscribeLocalEvent<HereticComponent, InteractHandEvent>(OnHereticInteract,
            after: [typeof(BuckleSystem)]);
        SubscribeLocalEvent<HereticComponent, BeforeDamageChangedEvent>(OnTakeDamage);
        SubscribeLocalEvent<HereticComponent, BeforeHarmfulActionEvent>(OnBeforeHarmfulAction,
            after: [typeof(HereticAbilitySystem), typeof(RiposteeSystem)]);
        SubscribeLocalEvent<HereticComponent, ProjectileReflectAttemptEvent>(OnProjectileReflectAttempt);
        SubscribeLocalEvent<HereticComponent, HitScanReflectAttemptEvent>(OnHitscanReflectAttempt);
    }

    private void OnHereticInteract(Entity<HereticComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled
            || args.User != ent.Owner)
            return;

        if (TryThrowProtectiveBlade(ent, null))
            args.Handled = true;
    }

    private void OnProjectileReflectAttempt(Entity<HereticComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (args.Cancelled
            || !TryComp<ReflectComponent>(ent, out var reflect))
            return;

        foreach (var blade in GetBlades(ent))
        {
            if (!_reflect.TryReflectProjectile((ent, reflect), blade, args.ProjUid))
                continue;

            args.Cancelled = true;
            RemoveProtectiveBlade(blade);
            break;
        }
    }

    private void OnHitscanReflectAttempt(Entity<HereticComponent> ent, ref HitScanReflectAttemptEvent args)
    {
        if (args.Reflected
            || !TryComp<ReflectComponent>(ent, out var reflect))
            return;

        foreach (var blade in GetBlades(ent))
        {
            if (!_reflect.TryReflectHitscan(
                    (ent, reflect),
                    blade,
                    args.Shooter,
                    args.SourceItem,
                    args.Direction,
                    args.Reflective,
                    args.Damage,
                    out var dir))
                continue;

            args.Direction = dir.Value;
            args.Reflected = true;
            RemoveProtectiveBlade(blade);
            break;
        }
    }

    private void OnBeforeHarmfulAction(Entity<HereticComponent> ent, ref BeforeHarmfulActionEvent args)
    {
        if (args.Cancelled)
            return;

        var blades = GetBlades(ent);
        if (blades.Count == 0)
            return;

        var blade = blades[0];
        RemoveProtectiveBlade(blade);

        _audio.PlayPvs(blade.Comp.BladeBlockSound, ent);

        args.Cancel();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProtectiveBladeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Timer -= frameTime;

            if (comp.Timer <= 0)
                RemoveProtectiveBlade((uid, comp));
        }
    }

    private static void OnInit(Entity<ProtectiveBladeComponent> ent, ref ComponentInit args) =>
        ent.Comp.Timer = ent.Comp.Lifetime;

    private void OnTakeDamage(Entity<HereticComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Cancelled
            || args.Damage.GetTotal() < 5f)
            return;

        var blades = GetBlades(ent);
        if (blades.Count == 0)
            return;

        var blade = blades[0];
        RemoveProtectiveBlade(blade);

        _audio.PlayPvs(blade.Comp.BladeBlockSound, ent);

        args.Cancelled = true;
    }

    private void OnInteract(Entity<ProtectiveBladeComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<FollowerComponent>(ent, out var follower)
            || follower.Following != args.User)
            return;

        if (TryThrowProtectiveBlade(args.User, ent))
            args.Handled = true;
    }

    public List<Entity<ProtectiveBladeComponent>> GetBlades(EntityUid ent)
    {
        var blades = new List<Entity<ProtectiveBladeComponent>>();

        if (!TryComp<FollowedComponent>(ent, out var followed))
            return blades;

        foreach (var following in followed.Following)
        {
            if (TryComp(following, out ProtectiveBladeComponent? blade))
                blades.Add((following, blade));
        }

        return blades;
    }
    private EntityUid? GetNearestTarget(EntityUid origin, float range = 10f)
    {
        var pos = _xform.GetWorldPosition(origin);

        var lookup = _lookup.GetEntitiesInRange(origin, range, flags: LookupFlags.Dynamic)
            .Where(ent => ent != origin && _mobState.IsAlive(ent) && _interaction.InRangeUnobstructed(
                origin,
                ent,
                range,
                CollisionGroup.BulletImpassable,
                x => TryComp(x, out RequireProjectileTargetComponent? requireTargetComp) && requireTargetComp.Active));

        float? nearestPoint = null;
        EntityUid? ret = null;
        foreach (var look in lookup)
        {
            var distance = (pos - _xform.GetWorldPosition(look)).Length();

            if (distance >= nearestPoint)
                continue;

            nearestPoint = distance;
            ret = look;
        }

        return ret;
    }

    public void AddProtectiveBlade(EntityUid ent, bool playSound = true)
    {
        var blade = Spawn(BladePrototype, Transform(ent).Coordinates);
        _follow.StartFollowingEntity(blade, ent);

        if (TryComp<ProtectiveBladeComponent>(blade, out var bladeComp)
            && playSound)
            _audio.PlayPvs(bladeComp.BladeAppearSound, ent);
    }
    public void RemoveProtectiveBlade(Entity<ProtectiveBladeComponent> blade)
    {
        if (!TryComp<FollowerComponent>(blade, out var follower))
            return;

        var ev = new ProtectiveBladeUsedEvent { Used = blade };
        RaiseLocalEvent(follower.Following, ev);

        QueueDel(blade);
    }

    public bool TryThrowProtectiveBlade(EntityUid origin, Entity<ProtectiveBladeComponent>? bladeNullable, EntityUid? specifiedTarget = null)
    {
        if (HasComp<BlockProtectiveBladeShootComponent>(origin))
            return false;

        if (bladeNullable is not { } blade)
        {
            var blades = GetBlades(origin);
            if (blades.Count == 0)
                return false;

            blade = blades[0];
        }

        _follow.StopFollowingEntity(origin, blade);

        var targetNullable = specifiedTarget ?? GetNearestTarget(origin);

        if (targetNullable is not { } target)
        {
            _popup.PopupEntity(Loc.GetString("heretic-protective-blade-component-no-targets"), origin, origin);
            return false;
        }

        var pos = _xform.GetWorldPosition(origin);
        var direction = _xform.GetWorldPosition(target) - pos;

        var projectile = Spawn(blade.Comp.BladeProjectilePrototype, Transform(origin).Coordinates);
        _gun.ShootProjectile(projectile, direction, Vector2.Zero, origin, origin);
        _gun.SetTarget(projectile, target, out _);

        var ev = new ProtectiveBladeUsedEvent { Used = blade };
        RaiseLocalEvent(origin, ev);

        QueueDel(blade);

        _status.TryAddStatusEffect<BlockProtectiveBladeShootComponent>(origin,
            "BlockProtectiveBladeShoot",
            TimeSpan.FromSeconds(0.5f),
            true);

        return true;
    }

    public void ThrowProtectiveBlade(EntityUid origin, EntityUid? target = null)
    {
        var blades = GetBlades(origin);

        if (blades.Count > 0)
            TryThrowProtectiveBlade(origin, blades[0], target);
    }

    public sealed class ProtectiveBladeUsedEvent : EntityEventArgs
    {
        public Entity<ProtectiveBladeComponent>? Used;
    }
}
