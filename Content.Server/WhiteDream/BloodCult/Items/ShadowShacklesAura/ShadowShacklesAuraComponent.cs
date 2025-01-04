using Content.Server.WhiteDream.BloodCult.Items.BaseAura;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Items.ShadowShacklesAura;

[RegisterComponent]
public sealed partial class ShadowShacklesAuraComponent : BaseAuraComponent
{
    [DataField]
    public EntProtoId ShacklesProto = "ShadowShackles";

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(1);
}
