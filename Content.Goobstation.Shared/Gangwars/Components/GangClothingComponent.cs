using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// Added to clothing items that can be painted and claimed by a gang.
/// When a gang member uses a GangSprayCanComponent entity on this entity,
/// it is assigned to their gang and its sprite is tinted to match
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color? Gang;
}
