// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag.Components;
using Content.Shared.GameTicking.Components;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Antag;

public sealed class AntagRandomSpawnSystem : GameRuleSystem<AntagRandomSpawnComponent>
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntagRandomSpawnComponent, AntagSelectLocationEvent>(OnSelectLocation);
    }

    protected override void Added(EntityUid uid, AntagRandomSpawnComponent comp, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, comp, gameRule, args);

        // we have to select this here because AntagSelectLocationEvent is raised twice because MakeAntag is called twice
        // once when a ghost role spawner is created and once when someone takes the ghost role

        if (TryFindRandomTile(out _, out _, out _, out var coords))
            comp.Coords = coords;
    }

    private void OnSelectLocation(Entity<AntagRandomSpawnComponent> ent, ref AntagSelectLocationEvent args)
    {
        if (ent.Comp.Coords != null)
        {
            args.Coordinates.Add(_transform.ToMapCoordinates(ent.Comp.Coords.Value));
            return;
        }

        // Goobstation Fallback: if nothing was pre-selected, try again now to avoid nullspace.
        if (TryFindRandomTile(out _, out _, out _, out var coords))
            args.Coordinates.Add(_transform.ToMapCoordinates(coords));
    }
}
