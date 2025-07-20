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

using Content.Server.Administration.Systems;
using Content.Shared._Lavaland.Aggression;
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Megafauna.Actions;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaSystem : SharedMegafaunaSystem
{
    [Dependency] private readonly AggressorsSystem _aggressors = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaStartupEvent>(OnMegafaunaStartup);
        SubscribeLocalEvent<MegafaunaAiComponent, MegafaunaShutdownEvent>(OnMegafaunaShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MegafaunaAiComponent, AggressiveComponent>();
        while (query.MoveNext(out var uid, out var ai, out var aggressive))
        {
            if (!ai.Active)
                continue;

            var schedule = new Dictionary<TimeSpan, MegafaunaActionSelector>(ai.ActionSchedule);
            foreach (var (time, action) in schedule)
            {
                if (time > Timing.CurTime)
                    continue;

                var args = new MegafaunaCalculationBaseArgs(uid, ai, EntityManager, _protoMan, Timing, _random.GetRandom());
                var delayTime = action.Invoke(args);
                ai.ActionSchedule.Remove(time);

                // We should spawn new actions only if there are no plans for new ones.
                // The Stack Overflow Defense
                if (ai.ActionSchedule.Count > 1)
                    continue;

                // Add new action
                delayTime = Math.Clamp(delayTime, ai.MinAttackCooldown, ai.MaxAttackCooldown);
                var nextAction = Timing.CurTime + TimeSpan.FromSeconds(delayTime);
                ai.ActionSchedule.TryAdd(nextAction, ai.Selector);

                // Pick new target
                _aggressors.TryPickTarget((uid, aggressive), out ai.CurrentTarget);
                ai.PreviousTarget = ai.CurrentTarget;
            }
        }
    }

    private void OnMegafaunaStartup(Entity<MegafaunaAiComponent> ent, ref MegafaunaStartupEvent args)
    {
        var nextAction = Timing.CurTime + TimeSpan.FromSeconds(ent.Comp.StartingCooldown);
        ent.Comp.ActionSchedule.TryAdd(nextAction, ent.Comp.Selector);
    }

    private void OnMegafaunaShutdown(Entity<MegafaunaAiComponent> ent, ref MegafaunaShutdownEvent args)
    {
        ent.Comp.ActionSchedule.Clear();
        if (ent.Comp.RejuvenateOnShutdown)
            _rejuvenate.PerformRejuvenate(ent);
    }
}
