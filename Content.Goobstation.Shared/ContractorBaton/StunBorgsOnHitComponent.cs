namespace Content.Goobstation.Shared.ContractorBaton;

[RegisterComponent]
public sealed partial class StunBorgsOnHitComponent : Component
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);
}
