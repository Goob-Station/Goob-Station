// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;

namespace Content.Shared.Chemistry.EntitySystems.Hypospray;

[RegisterComponent]
public sealed partial class SolutionCartridgeComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string TargetSolution = "default";

    [DataField(required: true)]
    public Solution Solution;
}