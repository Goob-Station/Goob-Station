using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shizophrenia;

/// <summary>
/// Component added to hallucinations to have access to main entity and visualization them on client
/// Hallucinations can't see or hear each other
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HallucinationComponent : Component
{
    /// <summary>
    /// Unique index for identifying hallucinations and their owner
    /// </summary>
    [AutoNetworkedField]
    public int Idx = 0;

    /// <summary>
    /// Hallucinating entity
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Ent;

    [DataField]
    public Color ChatColor = Color.FromHex("#b81500FF");
}
