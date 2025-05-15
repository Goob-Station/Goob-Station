// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Scruq445 <storchdamien@gmail.com>
// SPDX-FileCopyrightText: 2025 crasg <109207982+Scruq445@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Server.GameTicking.Events;
using Content.Shared.Tiles;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Ghostbar;

public sealed class AntagBaseSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;

    private bool _enabled;

    public override void Initialize()
    {
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);

        Subs.CVar(_cfg, GoobCVars.AntagPlanetEnabled, value => _enabled = value, true);
    }

    const string AntagMapDIR = "Maps/_Goobstation/Nonstations/antagplanet.yml";

    void OnRoundStart(RoundStartingEvent ev)
    {
        if (!_enabled)
            return;

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
