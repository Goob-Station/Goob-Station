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

using System.Diagnostics.CodeAnalysis;
using Content.Server._Lavaland.Aggression;
using Content.Server.Administration.Systems;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaSystem : SharedMegafaunaSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly AggressorsSystem _aggressors = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaAiComponent, AggressiveComponent>();
        while (query.MoveNext(out var uid, out var ai, out var aggressive))
        {
            if (ai.NextAction < Timing.CurTime
                || !ai.Active)
                continue;

            var args = new MegafaunaCalculationBaseArgs(uid, ai, EntityManager);

            // Fill action queue if it's less than buffer
            if (ai.ActionQueue.Count < ai.ActionQueueBufferSize)
            {
                for (int i = 0; i < ai.ActionQueueBufferSize - ai.ActionQueue.Count; i++)
                {
                    if (!TryPickMegafaunaAttack(args, out var picked))
                        continue;

                    ai.ActionQueue.Enqueue(picked);
                }
            }

            // Pick new target
            _aggressors.TryPickTarget((uid, aggressive), out ai.CurrentTarget);
            ai.PreviousTarget = ai.CurrentTarget;
        }
    }

    /// <summary>
    /// Picks megafauna attack for the megafauna AI, running conditions for each attack
    /// </summary>
    private bool TryPickMegafaunaAttack(MegafaunaCalculationBaseArgs args, [NotNullWhen(true)] out MegafaunaAction? action)
    {
        action = null;
        var comp = args.AiComponent;

        var actionsData = _protoMan.Index(comp.ActionsDataId);
        var conditionChecks = new List<MegafaunaCheckEntry>(actionsData.Entries.Count);
        foreach (var patternId in actionsData.Entries)
        {
            var pattern = _protoMan.Index(patternId);

            var failCount = 0;
            foreach (var condition in pattern.Conditions)
            {
                var condPassed = condition.Check(args);
                if (!condPassed)
                    failCount++;
            }

            conditionChecks.Add(new MegafaunaCheckEntry(pattern.Action, failCount));
        }

        conditionChecks = FilterByPrevious(conditionChecks, comp.PreviousAttack);
        conditionChecks = FilterByConditions(conditionChecks);

        return PickRandomMegafaunaAttack(conditionChecks, out action);
    }

    private List<MegafaunaCheckEntry> FilterByPrevious(List<MegafaunaCheckEntry> list, string? previousAttack)
    {
        if (previousAttack == null)
            return list;

        var passed = new List<MegafaunaCheckEntry>(list);
        foreach (var entry in list)
        {
            if (entry.Action.Name == previousAttack)
                passed.Remove(entry);
        }

        return passed.Count == 0 ? list : passed;
    }

    private List<MegafaunaCheckEntry> FilterByConditions(List<MegafaunaCheckEntry> list)
    {
        var leastFails = int.MaxValue;
        foreach (var (_, amount) in list)
        {
            if (leastFails > amount)
                leastFails = amount;
        }

        var passed = new List<MegafaunaCheckEntry>(list);
        foreach (var entry in list)
        {
            if (entry.FailAmout == leastFails)
                passed.Remove(entry);
        }

        return passed.Count == 0 ? list : passed;
    }

    private bool PickRandomMegafaunaAttack(
        List<MegafaunaCheckEntry> actionsData,
        [NotNullWhen(true)] out MegafaunaAction? attack)
    {
        attack = null;
        if (actionsData.Count == 0)
            return false;

        attack = _random.PickAndTake(actionsData).Action;
        return true;
    }

    private record struct MegafaunaCheckEntry(MegafaunaAction Action, int FailAmout);

    private void OnMegafaunaShutdown(Entity<MegafaunaAiComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (ent.Comp.RejuvenateOnShutdown)
            _rejuvenate.PerformRejuvenate(ent);
    }
}
