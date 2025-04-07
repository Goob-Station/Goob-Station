// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 amogus <113782077+whateverusername0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Mind;

namespace Content.Goobstation.Server.Pirates.GameTicking.Rules;

[RegisterComponent]
public sealed partial class ActivePirateRuleComponent : Component
{
    public List<Entity<MindComponent>> Pirates = new();
    [ViewVariables(VVAccess.ReadWrite)] public float Credits = 0f;
    [ViewVariables(VVAccess.ReadWrite)] public EntityUid? BoundSiphon;
}