namespace Content.Shared.Gravity;

public sealed partial class FloatingVisualsComponent
{
    /// <summary>
    /// If true, the entity will float regardless of gravity. Because it looks cool.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool FloatRegardlessOfGravity;
}
