// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Tools.Components;

namespace Content.Shared.Tools.Systems;

/// <summary>
///     Raised when <see cref="WeldableComponent"/> has changed.
/// </summary>
[ByRefEvent]
public readonly record struct WeldableChangedEvent(bool IsWelded)
{
    public readonly bool IsWelded = IsWelded;
}