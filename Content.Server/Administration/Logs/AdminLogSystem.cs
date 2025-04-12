// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;

namespace Content.Server.Administration.Logs;

/// <summary>
///     For system events that the manager needs to know about.
///     <see cref="IAdminLogManager"/> for admin log usage.
/// </summary>
public sealed class AdminLogSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogs = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(ev => _adminLogs.RoundStarting(ev.Id));
        SubscribeLocalEvent<GameRunLevelChangedEvent>(ev => _adminLogs.RunLevelChanged(ev.New));
    }

    public override void Update(float frameTime)
    {
        _adminLogs.Update();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _adminLogs.Shutdown();
    }
}