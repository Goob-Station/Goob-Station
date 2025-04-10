// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking.Events;
using Content.Shared.Tiles;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Ghostbar;

public sealed class AntagBaseSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    const string AntagMapDIR = "Maps/_Goobstation/Nonstations/antagplanet.yml";

    void OnRoundStart(RoundStartingEvent ev)
    {
        var DirPath = new ResPath(AntagMapDIR);

        if (_mapLoader.TryLoadMap(DirPath,
                out var MapStat,
                out _,
                new DeserializationOptions { InitializeMaps = true }))
        {
            _mapSystem.SetPaused(MapStat.Value.Comp.MapId, false);
            EnsureComp<ProtectedGridComponent>(MapStat.Value.Comp.Owner);
        }
    }
}
