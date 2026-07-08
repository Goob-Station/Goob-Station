// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent]
public sealed partial class ChangelingDartComponent : Component
{
    [DataField(required: true)]
    public ProtoId<ReagentStingConfigurationPrototype> StingConfiguration;

    [DataField]
    public float ReagentDivisor = 2;
}