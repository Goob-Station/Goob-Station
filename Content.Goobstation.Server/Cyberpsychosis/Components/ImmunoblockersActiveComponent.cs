namespace Content.Goobstation.Server.Cyberpsychosis;

/// <summary>
/// Component added to entities under the effects of immunoblockers, reducing their cyber sanity loss.
/// </summary>
[RegisterComponent]
public sealed partial class ImmunoblockersActiveComponent : Component
{
    [DataField]
    public float LossModifier = 0.5f;
}
