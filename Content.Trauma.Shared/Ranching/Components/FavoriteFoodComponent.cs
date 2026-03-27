using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FavoriteFoodComponent : Component
{
    [DataField]
    public HashSet<ProtoId<TagPrototype>> Tag = new ();

    [DataField]
    public int Amount = 30;
}
