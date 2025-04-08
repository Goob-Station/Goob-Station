// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.GameTicking;

namespace Content.Server.Polymorph.Systems;

public sealed partial class PolymorphSystem
{
    public EntityUid? PausedMap { get; private set; }

    /// <summary>
    /// Used to subscribe to the round restart event
    /// </summary>
    private void InitializeMap()
    {
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent _)
    {
        if (PausedMap == null || !Exists(PausedMap))
            return;

        Del(PausedMap.Value);
    }

    /// <summary>
    /// Used internally to ensure a paused map that is
    /// stores polymorphed entities.
    /// </summary>
    private void EnsurePausedMap()
    {
        if (PausedMap != null && Exists(PausedMap))
            return;

        var newmap = _mapManager.CreateMap();
        _mapManager.SetMapPaused(newmap, true);
        PausedMap = _mapManager.GetMapEntityId(newmap);
    }
}