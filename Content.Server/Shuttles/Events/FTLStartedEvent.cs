// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Map;

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised when a shuttle has moved to FTL space.
/// </summary>
[ByRefEvent]
public readonly record struct FTLStartedEvent(EntityUid Entity, EntityCoordinates TargetCoordinates, EntityUid? FromMapUid, Matrix3x2 FTLFrom, Angle FromRotation);