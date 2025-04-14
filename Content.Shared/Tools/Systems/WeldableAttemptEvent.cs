// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Tools.Systems;

/// <summary>
///     Checks that entity can be weld/unweld.
///     Raised twice: before do_after and after to check that entity still valid.
/// </summary>
public sealed class WeldableAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid User;
    public readonly EntityUid Tool;

    public WeldableAttemptEvent(EntityUid user, EntityUid tool)
    {
        User = user;
        Tool = tool;
    }
}