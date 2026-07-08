// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Lavaland.Damage.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Damage;

/// <summary>
///     We have to use our own system even for the damage field because WIZDEN SYSTEMS FUCKING SUUUUUUUUUUUCKKKKKKKKKKKKKKK
/// </summary>
public sealed class DamageSquareSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    private EntityQuery<DamageableComponent> _damageQuery;
    private EntityQuery<DamageSquareImmunityComponent> _immuneQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageSquareComponent, ComponentStartup>(OnMapInit);

        _damageQuery = GetEntityQuery<DamageableComponent>();
        _immuneQuery = GetEntityQuery<DamageSquareImmunityComponent>();
    }

    private void OnMapInit(Entity<DamageSquareComponent> ent, ref ComponentStartup args)
        => ent.Comp.DamageTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.DamageDelay);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var immuneQuery = EntityQueryEnumerator<DamageSquareImmunityComponent>();
        while (immuneQuery.MoveNext(out var uid, out var immune))
        {
            if (immune.ImmunityEndTime == null
                || _timing.CurTime < immune.ImmunityEndTime)
                continue;

            RemComp(uid, immune);
        }

        var query = EntityQueryEnumerator<DamageSquareComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var damage, out _))
        {
            if (_timing.CurTime < damage.DamageTime)
                continue;

            Damage((uid, damage));
        }
    }

    private void Damage(Entity<DamageSquareComponent> field)
    {
        var xform = Transform(field);
        if (xform.GridUid == null)
        {
            RemComp(field, field.Comp);
            return;
        }

        var grid = xform.GridUid.Value;
        var tile = _map.GetTileRef(grid, Comp<MapGridComponent>(grid), xform.Coordinates);

        var lookup = _lookup.GetLocalEntitiesIntersecting(tile, 0f);

        foreach (var target in lookup)
        {
            if (!_damageQuery.TryComp(target, out var damageable)
                || _immuneQuery.HasComp(target)
                || _whitelist.IsWhitelistFail(field.Comp.DamageWhitelist, target)
                || _whitelist.IsWhitelistFail(field.Comp.DamageBlacklist, target))
                continue;

            if (_net.IsServer) // Movement prediction is wonky and doesn't compensate for lag
            {
                _audio.PlayPvs(field.Comp.Sound, target);
                _damage.TryChangeDamage(target,
                    field.Comp.Damage,
                    damageable: damageable,
                    origin: field.Owner,
                    targetPart: TargetBodyPart.All);
            }

            EnsureComp<DamageSquareImmunityComponent>(target).ImmunityEndTime =
                _timing.CurTime + TimeSpan.FromSeconds(field.Comp.ImmunityTime);
        }

        RemComp(field, field.Comp);
    }
}
