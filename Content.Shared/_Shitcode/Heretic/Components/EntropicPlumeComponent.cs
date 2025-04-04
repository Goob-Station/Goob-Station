using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EntropicPlumeComponent : Component
{
    [DataField]
    public float Duration = 10f;

    [DataField]
    public Dictionary<string, FixedPoint2> Reagents = new()
    {
        { "Mold", 10f },
    };

    [DataField]
    public List<EntityUid> AffectedEntities = new();
}
