using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MouseSwarm;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MouseSwarmComponent : Component
{
    [DataField]
    public ResPath Sprite = new("/Textures/Mobs/Animals/mouse.rsi");

    [DataField]
    public string State = "mouse-0";

    [DataField]
    public DamageSpecifier DamagePerMember = new() { DamageDict = new() { { "Slash", 1 } } };

    [DataField]
    public int MaxMembers = 25;

    [DataField]
    public float MaxOffset = 0.6f;

    [DataField, AutoNetworkedField]
    public List<MouseSwarmMember> Members = new();
}

[Serializable, NetSerializable, DataDefinition]
public partial record struct MouseSwarmMember
{
    [DataField]
    public ResPath Sprite;

    [DataField]
    public string State;

    [DataField]
    public Vector2 Offset;

    [DataField]
    public EntProtoId? Prototype;

    public MouseSwarmMember(ResPath sprite, string state, Vector2 offset, EntProtoId? prototype)
    {
        Sprite = sprite;
        State = state;
        Offset = offset;
        Prototype = prototype;
    }
}
