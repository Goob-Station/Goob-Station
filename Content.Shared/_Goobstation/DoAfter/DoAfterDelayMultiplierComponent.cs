namespace Content.Shared._Goobstation.DoAfter;

[RegisterComponent]
public sealed partial class DoAfterDelayMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 1f;
}
