namespace Content.Goobstation.Server.Shizophrenia;

/// <summary>
/// Component added to currently hallucinating entities
/// </summary>
[RegisterComponent]
public sealed partial class HallucinatingComponent : Component
{
    /// <summary>
    /// Current hallucinations with their ids
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, BaseHallucinationsEntry> Hallucinations = new();

    /// <summary>
    /// Lifetimes for temporal hallucinations
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, TimeSpan> Removes = new();

    public TimeSpan NextUpdate = TimeSpan.Zero;
}

