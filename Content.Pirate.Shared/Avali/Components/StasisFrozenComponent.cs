using Content.Pirate.Shared.Avali.EntitySystems;
using Robust.Shared.GameStates;

namespace Content.Pirate.Shared.Avali.Components;

/// <summary>
/// Prevents an entity in stasis from performing most actions.
/// </summary>
[RegisterComponent, Access(typeof(SharedStasisFrozenSystem))]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StasisFrozenComponent : Component
{
    /// <summary>
    /// Whether speech and emotes are blocked while frozen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Muted;
}
