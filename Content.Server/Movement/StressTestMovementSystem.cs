// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Numerics;
using Content.Server.Movement.Components;

namespace Content.Server.Movement;

public sealed class StressTestMovementSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StressTestMovementComponent, ComponentStartup>(OnStressStartup);
    }

    private void OnStressStartup(EntityUid uid, StressTestMovementComponent component, ComponentStartup args)
    {
        component.Origin = _transform.GetWorldPosition(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StressTestMovementComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var stressTest, out var transform))
        {
            if (!transform.ParentUid.IsValid())
                continue;

            stressTest.Progress += frameTime;

            if (stressTest.Progress > 1)
            {
                stressTest.Progress -= 1;
            }

            var x = MathF.Sin(stressTest.Progress * MathHelper.TwoPi);
            var y = MathF.Cos(stressTest.Progress * MathHelper.TwoPi);

            _transform.SetWorldPosition((uid, transform), stressTest.Origin + new Vector2(x, y) * 5);
        }
    }
}