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

using System.Linq;
using Content.Shared._Lavaland.Aggression;

namespace Content.Server._Lavaland.Aggression;

public sealed class AggressorsSystem : SharedAggressorsSystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // All who are aggressive check their aggressors, and remove them if they are too far away.
        var query = EntityQueryEnumerator<AggressiveComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var aggressive, out var xform))
        {
            if (aggressive.ForgiveRange == null)
                continue;

            aggressive.Accumulator += frameTime;

            if (aggressive.Accumulator < aggressive.UpdateFrequency)
                continue;

            aggressive.Accumulator = 0f;

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
}
