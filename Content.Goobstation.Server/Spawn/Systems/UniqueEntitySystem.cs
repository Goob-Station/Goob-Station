// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Spawn.Components;
using Content.Server.Station.Systems;

namespace Content.Goobstation.Server.Spawn.Systems;

public sealed partial class UniqueEntitySystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UniqueEntityCheckerComponent, ComponentInit>(OnComponentInit);
    }

    public void OnComponentInit(Entity<UniqueEntityCheckerComponent> checker, ref ComponentInit args)
    {
        var comp = checker.Comp;

        if (string.IsNullOrEmpty(comp.MarkerName))
            return;

        var query = EntityQueryEnumerator<UniqueEntityMarkerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var marker, out var xform))
        {
            if (string.IsNullOrEmpty(marker.MarkerName)
                || marker.MarkerName != comp.MarkerName
                || uid == checker.Owner)
                continue;

            // Check if marker on station
            if (marker.StationOnly && _station.GetOwningStation(uid, xform) is null)
                continue;

            // Delete it if found unique entity
            QueueDel(checker);
            return;
        }
    }
}