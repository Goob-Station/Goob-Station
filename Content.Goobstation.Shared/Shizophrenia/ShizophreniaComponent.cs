using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shizophrenia;

/// <summary>
/// Component added to entities experiencing hallucinations
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SchizophreniaComponent : Component
{
    /// <summary>
    /// List of hallucination entities
    /// Server-only
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public List<EntityUid> Hallucinations = new();

    /// <summary>
    /// Unique index for component owner and their hallucinations
    /// Used for sentinent hallucinations to identify owner
    /// </summary>
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Idx = 0;
}
