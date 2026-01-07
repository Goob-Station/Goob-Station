using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PapaDoodonComponent : Component
{
    [DataField("commandAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string CommandAction = "PapaDoodonCommand";

    [DataField("commandActionEntity")]
    public EntityUid? CommandActionEntity;

    [AutoNetworkedField]
    [DataField("currentOrder")]
    public DoodonOrderType CurrentOrder = DoodonOrderType.Follow;
}

[Serializable, NetSerializable]
public enum DoodonOrderType : byte
{
    Stay,
    Follow,
    AttackTarget, // like CheeseEm (attack a chosen target)
    Loose         // free-roam aggro (SimpleHostile)
}
