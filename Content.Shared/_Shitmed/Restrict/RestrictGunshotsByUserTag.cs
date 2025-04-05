using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Restrict;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RestrictGunshotsByUserTagComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<TagPrototype>> Contains = [];

    [DataField, AutoNetworkedField]
    public List<ProtoId<TagPrototype>> DoesntContain = [];

    [DataField, AutoNetworkedField]
    public List<string> Messages = [];

    public TimeSpan LastPopup;
}
