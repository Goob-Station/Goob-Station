using Content.Server.WhiteDream.BloodCult.Items.BaseAura;
using Content.Shared.Chat;

namespace Content.Server.WhiteDream.BloodCult.Items.StunAura;

[RegisterComponent]
public sealed partial class StunAuraComponent : BaseAuraComponent
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(16);

    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(12);
}
