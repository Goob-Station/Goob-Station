using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Marker component for all entities that are being tracked by some megafauna AI.
/// Tracks them being deleted, in order to automatically update dictionaries and prevent
/// deleted entities being saved in <see cref="MegafaunaAiTargetingComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaTargetedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Targeted;
}
