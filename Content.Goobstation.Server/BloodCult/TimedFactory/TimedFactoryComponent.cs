using Content.Shared._White.RadialSelector;

namespace Content.Goobstation.Server.BloodCult.TimedFactory;

[RegisterComponent]
public sealed partial class TimedFactoryComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();

    [DataField]
    public float Cooldown = 240;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CooldownRemaining = 0;
}
