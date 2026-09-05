// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Wizard.Components;

[RegisterComponent]
public sealed partial class GrantTargetObjectiveOnGhostTakeoverComponent : Component
{
    [DataField]
    public EntityUid? TargetMind;

    [DataField(required: true)]
    public EntProtoId Objective;
}