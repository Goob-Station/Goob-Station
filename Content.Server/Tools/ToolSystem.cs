// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.FixedPoint;
using Content.Shared.Tools.Components;
using Robust.Server.GameObjects;

using SharedToolSystem = Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Server.Tools;

public sealed class ToolSystem : SharedToolSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void TurnOn(Entity<WelderComponent> entity, EntityUid? user)
    {
        base.TurnOn(entity, user);
        var xform = Transform(entity);
        if (xform.GridUid is { } gridUid)
        {
            var position = _transformSystem.GetGridOrMapTilePosition(entity.Owner, xform);
            _atmosphereSystem.HotspotExpose(gridUid, position, 700, 50, entity.Owner, true);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateWelders(frameTime);
    }

    //todo move to shared once you can remove reagents from shared without it freaking out.
    private void UpdateWelders(float frameTime)
    {
        var query = EntityQueryEnumerator<WelderComponent, SolutionContainerManagerComponent>();
        while (query.MoveNext(out var uid, out var welder, out var solutionContainer))
        {
            if (!welder.Enabled)
                continue;

            welder.WelderTimer += frameTime;

            if (welder.WelderTimer < welder.WelderUpdateTimer)
                continue;

            if (!SolutionContainerSystem.TryGetSolution((uid, solutionContainer), welder.FuelSolutionName, out var solutionComp, out var solution))
                continue;

            SolutionContainerSystem.RemoveReagent(solutionComp.Value, welder.FuelReagent, welder.FuelConsumption * welder.WelderTimer);

            if (solution.GetTotalPrototypeQuantity(welder.FuelReagent) <= FixedPoint2.Zero)
            {
                ItemToggle.Toggle(uid, predicted: false);
            }

            Dirty(uid, welder);
            welder.WelderTimer -= welder.WelderUpdateTimer;
        }
    }
}
