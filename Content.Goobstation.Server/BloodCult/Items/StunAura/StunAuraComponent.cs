using Content.Server.WhiteDream.BloodCult.Items.BaseAura;

namespace Content.Goobstation.Server.BloodCult.Items.StunAura;

[RegisterComponent]
public sealed partial class StunAuraComponent : BaseAuraComponent
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);
}
