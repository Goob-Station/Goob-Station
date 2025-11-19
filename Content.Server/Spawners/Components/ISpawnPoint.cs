// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Spawners.Components;

public interface ISpawnPoint
{
    SpawnPointType SpawnType { get; set; }
}

