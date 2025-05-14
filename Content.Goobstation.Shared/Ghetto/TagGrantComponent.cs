using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ghetto;

[RegisterComponent, NetworkedComponent]
public sealed partial class TagGrantComponent : Component
{
    [DataField("uses")]
    public int Uses = 1;

    [DataField("tag")]
    public ProtoId<TagPrototype> Tag = string.Empty;

    [DataField("popup")]
    public LocId Popup = string.Empty;
}
