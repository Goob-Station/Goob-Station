namespace Content.Goobstation.Common.MisandryBox;

[RegisterComponent]
public sealed partial class IgniteMultiplierComponent : Component
{
    /// <summary>
    /// Multiply received burn stacks by this value
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Factor { get; set; } = 2;
}
