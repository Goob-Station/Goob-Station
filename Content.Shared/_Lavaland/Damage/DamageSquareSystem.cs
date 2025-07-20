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

using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Damage;

/// <summary>
///     We have to use it's own system even for the damage field because WIZDEN SYSTEMS FUCKING SUUUUUUUUUUUCKKKKKKKKKKKKKKK
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageSquareComponent, ComponentStartup>(OnMapInit);
        SubscribeLocalEvent<DamageImmunityFramesComponent, BeforeDamageChangedEvent>(BeforeDamage);

        _damageQuery = GetEntityQuery<DamageableComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<DamageSquareComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var damage, out _))
        {
            if (_timing.CurTime < damage.DamageTime)
                continue;

            Damage((uid, damage));
        }
    }

    private void OnMapInit(Entity<DamageSquareComponent> ent, ref ComponentStartup args)
        => ent.Comp.DamageTime = _timing.CurTime + TimeSpan.FromSeconds(ent.Comp.DamageDelay);

    private void BeforeDamage(Entity<DamageImmunityFramesComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.CanBeCancelled
            && HasImmunity((ent.Owner, ent.Comp)))
            args.Cancelled = true;
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
                || HasImmunity(target)
                || _whitelist.IsWhitelistFail(field.Comp.DamageWhitelist, target)
                || _whitelist.IsBlacklistPass(field.Comp.DamageBlacklist, target))
                continue;

            _audio.PlayPredicted(field.Comp.Sound, target, target);
            if (_net.IsServer) // One must imagine DamageableSystem prediction.
                _damage.TryChangeDamage(target, field.Comp.Damage, damageable: damageable, origin: field.Owner, targetPart: TargetBodyPart.All);

            EnsureComp<DamageImmunityFramesComponent>(target).HasImmunityUntil =
                _timing.CurTime + TimeSpan.FromSeconds(field.Comp.ImmunityTime);
        }

        RemComp(field, field.Comp);
    }

    private bool HasImmunity(Entity<DamageImmunityFramesComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        return ent.Comp.HasImmunityUntil > _timing.CurTime;
    }
}
