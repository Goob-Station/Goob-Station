// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.PendingAntag;

public sealed class PendingAntagSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly AntagSelectionSystem _selection = default!;

    public Dictionary<NetUserId, (AntagSelectionDefinition, Entity<AntagSelectionComponent>)> PendingAntags = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.LateJoin)
            return;

        if (ev.JobId == null || !_prototypeManager.Index<JobPrototype>(ev.JobId).CanBeAntag)
            return;

        if (!PendingAntags.Remove(ev.Player.UserId, out var pendingAntag))
            return;

        _selection.TryMakeAntag(pendingAntag.Item2, ev.Player, pendingAntag.Item1, true);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        PendingAntags.Clear();
    }
}