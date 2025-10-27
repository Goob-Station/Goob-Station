// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Magic;

// TODO: If still needed, move to magic component
[ImplicitDataDefinitionForInheritors]
public abstract partial class MagicInstantSpawnData;

/// <summary>
/// Spawns underneath caster.
/// </summary>
public sealed partial class TargetCasterPos : MagicInstantSpawnData;

/// <summary>
/// Spawns 3 tiles wide in front of the caster.
/// </summary>
public sealed partial class TargetInFront : MagicInstantSpawnData
{
    [DataField]
    public int Width = 3;
}


/// <summary>
/// Spawns 1 tile in front of caster
/// </summary>
public sealed partial class TargetInFrontSingle : MagicInstantSpawnData;