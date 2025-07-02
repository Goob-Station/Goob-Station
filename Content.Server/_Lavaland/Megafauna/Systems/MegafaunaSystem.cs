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

using Content.Server._Lavaland.Aggression;
using Content.Server._Lavaland.Megafauna.Components;
using Content.Server.Administration.Systems;
using Content.Shared._Lavaland.Aggression;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Random;

// ReSharper disable EnforceForStatementBraces
// ReSharper disable EntityNameCapturedOnly.Local
namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed partial class MegafaunaSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AggressorsSystem _aggressors = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly MobThresholdSystem _threshold = default!;

    private EntityQuery<AggressiveMegafaunaAiComponent> _agressiveQuery;
    private EntityQuery<PhasesMegafaunaAiComponent> _phasesQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorAddedEvent>(OnAggressorAdded);
        SubscribeLocalEvent<MegafaunaAiComponent, AggressorRemovedEvent>(OnAggressorRemoved);

        _agressiveQuery = GetEntityQuery<AggressiveMegafaunaAiComponent>();
        _phasesQuery = GetEntityQuery<PhasesMegafaunaAiComponent>();

        InitializePhases();
        InitializeAggression();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaAiComponent, AggressiveComponent>();
        while (query.MoveNext(out var uid, out var ai, out var aggressive))
        {
            if (!ai.Active)
                continue;

            ai.NextAttackAccumulator -= frameTime;

            if (ai.NextAttackAccumulator > 0f)
                continue;

            // Pick the attack
            var args = new MegafaunaThinkBaseArgs(uid, ai, EntityManager);
            if (!TryPickMegafaunaAttack(args, out var attack))
                continue;

            // Pick the target
            ai.PreviousTarget = ai.CurrentTarget;
            _aggressors.TryPickTarget((uid, aggressive), out ai.CurrentTarget);

            // Make action, write that we used it and go on to delay
            var delayTime = attack.Invoke(args);

            ai.PreviousAttack = null;
            if (!ai.CanRepeatAttacks)
                ai.PreviousAttack = attack.Name;

            delayTime = Math.Clamp(delayTime, ai.MinAttackCooldown, ai.MaxAttackCooldown);
            ai.NextAttackAccumulator = delayTime;
        }

        UpdatePhases(frameTime);
        UpdateAggression(frameTime);
    }

    #region Event Handling

    private void OnAggressorAdded(Entity<MegafaunaAiComponent> ent, ref AggressorAddedEvent args)
    {
        if (!TryComp<AggressiveComponent>(ent, out var aggressive))
            return;

        if (!ent.Comp.Active)
        {
            ent.Comp.Active = true;
            RaiseLocalEvent(ent, new MegafaunaStartupEvent());
        }

        UpdateScaledThresholds((ent, ent.Comp, aggressive));
    }

    private void OnAggressorRemoved(Entity<MegafaunaAiComponent> ent, ref AggressorRemovedEvent args)
    {
        if (!TryComp<AggressiveComponent>(ent, out var aggressive))
            return;

        if (ent.Comp.Active && aggressive.Aggressors.Count == 0)
        {
            ent.Comp.Active = false;
            RaiseLocalEvent(ent, new MegafaunaShutdownEvent());
        }

        UpdateScaledThresholds((ent, ent.Comp, aggressive));
    }

    private void OnMegafaunaStartup(Entity<MegafaunaAiComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (!_threshold.TryGetDeadThreshold(ent.Owner, out var threshold))
        {
            Log.Error($"Megafauna {ToPrettyString(ent)} didn't have MobThresholdComponent when trying to startup a boss!");
            return;
        }

        ent.Comp.BaseTotalHp = threshold.Value;
    }

    private void OnMegafaunaShutdown(Entity<MegafaunaAiComponent> ent, ref MegafaunaShutdownEvent args)
    {
        _threshold.SetMobStateThreshold(ent, ent.Comp.BaseTotalHp, MobState.Dead);

        if (ent.Comp.RejuvenateOnShutdown)
            _rejuvenate.PerformRejuvenate(ent);
    }

    #endregion
}
