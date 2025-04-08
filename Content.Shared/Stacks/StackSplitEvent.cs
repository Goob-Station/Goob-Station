// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Stacks;

/// <summary>
///     Raised on the original stack entity when it is split to create another.
/// </summary>
/// <param name="NewId">The entity id of the new stack.</param>
[ByRefEvent]
public readonly record struct StackSplitEvent(EntityUid NewId);