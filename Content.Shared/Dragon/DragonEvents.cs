// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.Dragon;

public sealed partial class DragonDevourActionEvent : EntityTargetActionEvent
{
}

public sealed partial class DragonSpawnRiftActionEvent : InstantActionEvent
{
}

/// <summary>
/// Goobstation
/// </summary>
public sealed partial class DragonSpawnCarpHordeActionEvent : InstantActionEvent;

/// <summary>
/// Goobstation
/// </summary>
public sealed partial class DragonRoarActionEvent : InstantActionEvent;
