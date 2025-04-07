// SPDX-FileCopyrightText: 2024 778b <33431126+778b@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Server.Spawners.Components;

public interface ISpawnPoint
{
    SpawnPointType SpawnType { get; set; }
}
