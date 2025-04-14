// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using Content.Server.Afk;
using Content.Server.GameTicking.Events;
using Content.Shared.GameTicking;

namespace Content.Goobstation.Server.MisandryBox.GrassEnforce;

/// <summary>
/// Connects <see cref="GrassEnforcementManager"/> to the simulation.
/// </summary>
public sealed class GrassEnforcementSystem : EntitySystem
{
    [Dependency] private readonly IGrassEnforcementManager _grass = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnREnd);
    }

    private void OnREnd(RoundRestartCleanupEvent ev)
    {
        _grass.RoundEnd();
    }

}
