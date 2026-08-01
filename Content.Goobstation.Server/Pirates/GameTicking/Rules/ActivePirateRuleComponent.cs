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