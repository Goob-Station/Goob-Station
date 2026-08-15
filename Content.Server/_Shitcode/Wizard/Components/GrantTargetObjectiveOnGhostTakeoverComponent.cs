// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class GrantTargetObjectiveOnGhostTakeoverComponent : Component
{
    [DataField]
    public EntityUid? TargetMind;

    [DataField(required: true)]
    public EntProtoId Objective;
}