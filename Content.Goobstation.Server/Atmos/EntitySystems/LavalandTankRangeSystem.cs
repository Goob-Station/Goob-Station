// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Atmos.Events;
using Content.Server._Lavaland.Procedural.Components;

namespace Content.Goobstation.Server.Atmos.Systems;

/// <summary>
/// System to make atmos bombs have uncapped range on lavaland.
/// </summary>
public sealed class LavalandTankRangeSystem : EntitySystem
{
    private EntityQuery<LavalandMapComponent> _lavalandQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lavalandQuery = GetEntityQuery<LavalandMapComponent>();

        SubscribeLocalEvent<TransformComponent, GasTankGetRangeEvent>(OnGetRange);
    }

    private void OnGetRange(Entity<TransformComponent> ent, ref GasTankGetRangeEvent args)
    {
        if (_lavalandQuery.HasComponent(ent.Comp.MapUid))
            args.MaxRange = float.MaxValue;
    }
}
