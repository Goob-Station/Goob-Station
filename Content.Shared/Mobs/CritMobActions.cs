// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.Mobs;

/// <summary>
///     Only applies to mobs in crit capable of ghosting/succumbing
/// </summary>
public sealed partial class CritSuccumbEvent : InstantActionEvent
{
}

/// <summary>
///     Only applies/has functionality to mobs in crit that have <see cref="DeathgaspComponent"/>
/// </summary>
public sealed partial class CritFakeDeathEvent : InstantActionEvent
{
}

/// <summary>
///     Only applies to mobs capable of speaking, as a last resort in crit
/// </summary>
public sealed partial class CritLastWordsEvent : InstantActionEvent
{
}