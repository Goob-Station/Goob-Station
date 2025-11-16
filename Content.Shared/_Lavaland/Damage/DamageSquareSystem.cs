// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aineias1 <dmitri.s.kiselev@gmail.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Milon <plmilonpl@gmail.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Unlumination <144041835+Unlumy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
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

        var lookup = _lookup.GetLocalEntitiesIntersecting(tile, 0f, LookupFlags.Uncontained);

        foreach (var target in lookup)
        {
            if (!_damageQuery.TryComp(target, out var damageable)
                || _immuneQuery.HasComp(target)
                || _whitelist.IsWhitelistFail(field.Comp.DamageWhitelist, target)
                || _whitelist.IsBlacklistPass(field.Comp.DamageBlacklist, target))
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
