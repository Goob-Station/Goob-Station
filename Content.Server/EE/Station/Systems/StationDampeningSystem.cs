// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Station.Events;
using Content.Shared.Physics;

namespace Content.Server.Station.Systems;

public sealed class StationDampeningSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<StationPostInitEvent>(OnInitStation);
    }

    private void OnInitStation(ref StationPostInitEvent ev)
    {
        foreach (var grid in ev.Station.Comp.Grids)
        {
            // If the station grid doesn't have defined dampening, give it a small dampening by default
            // This will ensure cargo tech pros won't fling the station 1000 megaparsec away from the galaxy
            if (!TryComp<PassiveDampeningComponent>(grid, out var dampening))
            {
                dampening = AddComp<PassiveDampeningComponent>(grid);
                dampening.Enabled = true;
                dampening.LinearDampening = 0.01f;
                dampening.AngularDampening = 0.01f;
            }
        }
    }
}