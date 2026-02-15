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

using Content.Shared._Lavaland.Audio;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Aggression;

public sealed class AggressorsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedBossMusicSystem _bossMusic = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AggressiveComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<AggressiveComponent, MobStateChangedEvent>(OnStateChange);

        SubscribeLocalEvent<AggressorComponent, MobStateChangedEvent>(OnAggressorStateChange);
        SubscribeLocalEvent<AggressorComponent, EntityTerminatingEvent>(OnAggressorDeleted);
        SubscribeLocalEvent<AggressorComponent, AggressiveAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<AggressorComponent, AggressiveRemovedEvent>(OnAggressorRemoved);

        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;

        // All who are aggressive check their aggressors, and remove them if they are too far away.
        var query = EntityQueryEnumerator<AggressiveComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var aggressive, out var xform))
        {
            if (aggressive.ForgiveRange == null
                || aggressive.NextUpdate < curTime)
                continue;

            aggressive.NextUpdate = curTime + aggressive.UpdateDelay;

            foreach (var aggressor in aggressive.Aggressors)
            {
                if (!_xformQuery.TryComp(aggressor, out var aggroXform))
                    continue;

                var aggroPos = _xform.GetWorldPosition(aggroXform);
                var aggressivePos = _xform.GetWorldPosition(xform);
                var distance = (aggressivePos - aggroPos).Length();

                if (distance > aggressive.ForgiveRange
                    || xform.MapID != aggroXform.MapID)
                    RemoveAggressor((uid, aggressive), aggressor);
            }
        }
    }

    #region Event Handling

    private void OnDamageChanged(Entity<AggressiveComponent> ent, ref DamageChangedEvent args)
    {
        var aggro = args.Origin;

        if (aggro == null
            || !HasComp<ActorComponent>(aggro))
            return;

        AddAggressor(ent, aggro.Value);
    }

    private void OnDeleted(Entity<AggressiveComponent> ent, ref EntityTerminatingEvent args)
        => RemoveAllAggressors(ent);

    private void OnStateChange(Entity<AggressiveComponent> ent, ref MobStateChangedEvent args)
        => RemoveAllAggressors(ent);

    private void OnAggressorAdded(Entity<AggressorComponent> ent, ref AggressiveAddedEvent args)
    {
        if (ent.Comp.Aggressives.TryFirstOrNull(out var boss))
            _bossMusic.StartBossMusic(boss.Value);
    }

    private void OnAggressorRemoved(Entity<AggressorComponent> ent, ref AggressiveRemovedEvent args)
        => _bossMusic.EndAllMusic(); // Stop the music if we are no longer get attacked by anyone.

    private void OnAggressorStateChange(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
    {
        if (_mobState.IsDead(ent.Owner))
            CleanAggressions((ent.Owner, ent.Comp));
    }

    private void OnAggressorDeleted(Entity<AggressorComponent> ent, ref EntityTerminatingEvent args)
        => CleanAggressions((ent.Owner, ent.Comp));

    #endregion

    #region Aggressive API

    public void AddAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        var (uid, comp) = ent;
        ent.Comp.Aggressors.Add(aggressor);

        var aggComp = EnsureComp<AggressorComponent>(aggressor);
        aggComp.Aggressives.Add(uid);

        RaiseLocalEvent(uid, new AggressorAddedEvent(aggressor));
        RaiseLocalEvent(aggressor, new AggressiveAddedEvent(uid));

        Dirty(uid, comp);
        Dirty(aggressor, aggComp);
    }

    public void RemoveAggressor(Entity<AggressiveComponent> ent, Entity<AggressorComponent?> aggressor)
    {
        if (!Resolve(aggressor, ref aggressor.Comp))
            return;

        ent.Comp.Aggressors.Remove(aggressor);
        aggressor.Comp.Aggressives.Remove(ent);

        if (aggressor.Comp.Aggressives.Count == 0)
            RemComp(aggressor, aggressor.Comp);
    }

    public void RemoveAllAggressors(Entity<AggressiveComponent> ent)
    {
        foreach (var aggressor in ent.Comp.Aggressors)
        {
            if (!TryComp<AggressorComponent>(aggressor, out var aggressorComp))
                continue;

            aggressorComp.Aggressives.Remove(ent.Owner);
            if (aggressorComp.Aggressives.Count == 0)
            {
                RaiseLocalEvent(aggressor, new AggressiveRemovedEvent(ent.Owner));
                RemComp(aggressor, aggressorComp);
            }
        }

        ent.Comp.Aggressors.Clear();
    }

    #endregion

    #region Aggressor API

    public void CleanAggressions(Entity<AggressorComponent?> aggressor)
    {
        if (!Resolve(aggressor, ref aggressor.Comp))
            return;

        foreach (var aggressive in aggressor.Comp.Aggressives)
        {
            if (TryComp<AggressiveComponent>(aggressive, out var aggressors))
                RemoveAggressor((aggressive, aggressors), aggressor);
        }

        RemComp(aggressor, aggressor.Comp);
    }

    #endregion
}
