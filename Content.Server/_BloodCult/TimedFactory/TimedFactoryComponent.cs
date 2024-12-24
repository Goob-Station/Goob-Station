using Content.Shared.RadialSelector;

namespace Content.Server.WhiteDream.BloodCult.TimedFactory;

[RegisterComponent]
public sealed partial class TimedFactoryComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();

    [DataField]
    public float Cooldown = 300;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CooldownRemaining = 0;
}
