// SPDX-FileCopyrightText: 2023 Vyacheslav Kovalevsky <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;

namespace Content.Shared.Throwing;

/// <summary>
///     Raised on thrown entity.
/// </summary>
[PublicAPI]
[ByRefEvent]
public readonly record struct ThrownEvent(EntityUid? User, EntityUid Thrown);