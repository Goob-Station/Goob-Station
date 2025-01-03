namespace Content.Server._Goobstation.Wizard;

/// <summary>
/// This component is required to make sure an entity is struck by the same lightning no more than once
/// </summary>
[RegisterComponent]
public sealed partial class StruckByLightningComponent : Component
{
    /// <summary>
    /// Indices of lightning beams that have struck this entity
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<ushort> BeamIndices = new();

    /// <summary>
    /// This component is removed when it reaches zero.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float Lifetime = 4f;
}
