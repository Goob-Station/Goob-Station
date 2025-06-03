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
using Content.Shared._Lavaland.Components;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Shared._Lavaland.Aggression;

public abstract class SharedAggressorsSystem : EntitySystem
{
    [Dependency] private readonly SharedBossMusicSystem _bossMusic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AggressiveComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
        SubscribeLocalEvent<AggressiveComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<AggressiveComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<AggressiveComponent, MobStateChangedEvent>(OnStateChange);
        SubscribeLocalEvent<AggressiveComponent, ComponentGetState>(OnAgressiveGetState);

        SubscribeLocalEvent<AggressorComponent, MobStateChangedEvent>(OnAggressorStateChange);
        SubscribeLocalEvent<AggressorComponent, EntityTerminatingEvent>(OnAggressorDeleted);
        SubscribeLocalEvent<AggressorComponent, ComponentRemove>(OnAggressorRemoved);
    }

    #region Event Handling

    private void OnAgressiveGetState(EntityUid uid, AggressiveComponent component, ref ComponentGetState args) =>
        args.State = new AggressiveComponentState(GetNetEntitySet(component.Aggressors));

    private void OnBeforeDamageChanged(Entity<AggressiveComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (args.Origin == null
            || HasComp<UnmannedWeaponryComponent>(args.Origin.Value))
            args.Cancelled = true;
    }

    private void OnDamageChanged(Entity<AggressiveComponent> ent, ref DamageChangedEvent args)
    {
        var aggro = args.Origin;

        if (aggro == null || !HasComp<ActorComponent>(aggro))
            return;

        AddAggressor(ent, aggro.Value);
    }

    private void OnAggressorRemoved(Entity<AggressorComponent> ent, ref ComponentRemove args)
    {
        _bossMusic.EndAllMusic(); // Stop the music if we no longer get attacked by anyone.
    }

    private void OnAggressorStateChange(Entity<AggressorComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            CleanAggressions((ent.Owner, ent.Comp));
    }

    private void OnAggressorDeleted(Entity<AggressorComponent> ent, ref EntityTerminatingEvent args)
    {
        CleanAggressions((ent.Owner, ent.Comp));
    }

    private void OnDeleted(Entity<AggressiveComponent> ent, ref EntityTerminatingEvent args)
    {
        RemoveAllAggressors(ent);
    }

    private void OnStateChange(Entity<AggressiveComponent> ent, ref MobStateChangedEvent args)
    {
        RemoveAllAggressors(ent);
    }

    #endregion

    #region Aggressive API

    public void AddAggressor(Entity<AggressiveComponent> ent, EntityUid aggressor)
    {
        ent.Comp.Aggressors.Add(aggressor);

        var aggcomp = EnsureComp<AggressorComponent>(aggressor);
        RaiseLocalEvent(ent, new AggressorAddedEvent(GetNetEntity(aggressor)));

        aggcomp.Aggressives.Add(ent);

        _bossMusic.StartBossMusic(ent.Owner);
        Dirty(ent.Owner, ent.Comp); // freaky but works.
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

            aggressorComp.Aggressives.Remove(ent);
            if (aggressorComp.Aggressives.Count == 0)
                RemComp(aggressor, aggressorComp);
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
