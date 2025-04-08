// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Plykiya <58439124+Plykiya@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Botany.Components;
using Content.Server.Kitchen.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Random;
using Robust.Shared.Containers;

namespace Content.Server.Botany.Systems;

public sealed class LogSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly RandomHelperSystem _randomHelper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LogComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, LogComponent component, InteractUsingEvent args)
    {
        if (!HasComp<SharpComponent>(args.Used))
            return;

        // if in some container, try pick up, else just drop to world
        var inContainer = _containerSystem.IsEntityInContainer(uid);
        var pos = Transform(uid).Coordinates;

        for (var i = 0; i < component.SpawnCount; i++)
        {
            var plank = Spawn(component.SpawnedPrototype, pos);

            if (inContainer)
                _handsSystem.PickupOrDrop(args.User, plank);
            else
            {
                var xform = Transform(plank);
                _containerSystem.AttachParentToContainerOrGrid((plank, xform));
                xform.LocalRotation = 0;
                _randomHelper.RandomOffset(plank, 0.25f);
            }
        }

        QueueDel(uid);
        args.Handled = true;
    }
}