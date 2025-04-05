using Content.Shared._Shitmed.Antags.Abductor;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Restrict;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RestrictByUserTagComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<TagPrototype>> Contains = [];

    [DataField, AutoNetworkedField]
    public List<ProtoId<TagPrototype>> DoesntContain = [];

    [DataField, AutoNetworkedField]
    public List<string> Messages = [];

    [DataField]
    public bool BlockInteraction = true;

    [DataField]
    public bool BlockMelee = true;

    [DataField]
    public bool BlockGunshots = true;

    public TimeSpan LastPopup;
}
