// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Decapoids;

[RegisterComponent]
[AutoGenerateComponentPause]
public sealed partial class VaporizerComponent : Component
{
    [DataField]
    public string LiquidTank = "waterTank";

    [DataField]
    public ProtoId<ReagentPrototype> ExpectedReagent = "Water";

    [DataField]
    public Gas OutputGas = Gas.WaterVapor;

    [DataField]
    public float MaxPressure = Atmospherics.OneAtmosphere * 10;

    [DataField]
    public float ReagentToMoles = 0.07f;

    [DataField]
    public float ReagentPerSecond = 0.09f;

    [DataField]
    public TimeSpan ProcessDelay = TimeSpan.FromSeconds(1);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextProcess;
}
