using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Heretic.Effects;

[RegisterComponent, NetworkedComponent]
public sealed partial class CrucibleSoulStatusEffectComponent : Component
{
    [DataField]
    public EntityCoordinates? Coords;
}
