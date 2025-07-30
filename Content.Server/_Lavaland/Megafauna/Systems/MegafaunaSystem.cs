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
using Content.Shared._Lavaland.Megafauna;
using Content.Shared._Lavaland.Megafauna.Actions;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._Lavaland.Megafauna.Systems;

public sealed class MegafaunaSystem : SharedMegafaunaSystem
{
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ISerializationManager _serializeMan = default!;
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

        var query = EntityQueryEnumerator<MegafaunaAiComponent>();
        while (query.MoveNext(out var uid, out var ai))
        {
            if (!ai.Active)
                continue;

            // TODO MEGAFAUNA actual multi-threading here
            for (var index = 0; index < ai.Threads.Count; index++)
            {
                var (thread, isMain) = ai.Threads[index];
                float? actionTime = null;
                var args = new MegafaunaCalculationBaseArgs(uid,
                    ai,
                    EntityManager,
                    _protoMan,
                    _serializeMan,
                    _random.GetRandom());

                foreach (var (time, action) in thread)
                {
                    if (time > Timing.CurTime)
                        continue;

                    actionTime = action.Invoke(args);
                    thread.Remove(time);
                    break;
                }

                if (!isMain
                    || actionTime == null)
                    continue;

                // Add next action to this thread
                actionTime = Math.Abs(actionTime.Value);
                var delayTime = ai.ActionDelaySelector.Get(args);
                AddActionToThread(ai, index, ai.Selector, actionTime.Value + delayTime);
            }

            // Dispose all empty threads at the end of the tick.
            ai.Threads.RemoveAll(x => x.Actions.Count == 0);
        }
    }

    private void OnMegafaunaStartup(Entity<MegafaunaAiComponent> ent, ref MegafaunaStartupEvent args)
    {
        var thread = new Dictionary<TimeSpan, MegafaunaActionSelector>();
        var nextAction = Timing.CurTime + TimeSpan.FromSeconds(ent.Comp.StartingDelay);
        thread.Add(nextAction, ent.Comp.Selector);
        ent.Comp.Threads.Add(new MegafaunaActionThread(thread, true));
    }

    private void OnMegafaunaShutdown(Entity<MegafaunaAiComponent> ent, ref MegafaunaShutdownEvent args)
    {
        ent.Comp.Threads.Clear();

        if (ent.Comp.RejuvenateOnShutdown)
            _rejuvenate.PerformRejuvenate(ent);
    }
}
