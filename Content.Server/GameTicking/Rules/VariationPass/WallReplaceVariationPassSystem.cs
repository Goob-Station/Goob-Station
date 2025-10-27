// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking.Rules.VariationPass.Components;
using Content.Server.GameTicking.Rules.VariationPass.Components.ReplacementMarkers;

namespace Content.Server.GameTicking.Rules.VariationPass;

/// <summary>
/// This handles the ability to replace entities marked with <see cref="WallReplacementMarkerComponent"/> in a variation pass
/// </summary>
public sealed class WallReplaceVariationPassSystem : BaseEntityReplaceVariationPassSystem<WallReplacementMarkerComponent, WallReplaceVariationPassComponent>
{
}