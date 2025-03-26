namespace Content.Shared._Goobstation.ContractorBaton;

[RegisterComponent]
public sealed partial class StunBorgsOnHitComponent : Component
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);
}
