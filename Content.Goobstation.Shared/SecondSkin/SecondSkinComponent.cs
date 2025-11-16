using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SecondSkin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SecondSkinComponent : Component
{
    [DataField(required: true)]
    public Color Color;

    [ViewVariables]
    public bool IsActive => User != null;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField]
    public SoundSpecifier SoundEquip =
        new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_transform.ogg");

    [DataField]
    public SoundSpecifier SoundUnequip =
        new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_strip.ogg");

    [DataField]
    public DamageSpecifier DamageToSilicons = new();

    [DataField]
    public float UpdateTime = 1f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Accumulator;

    [DataField]
    public TargetBodyPart Parts = TargetBodyPart.FullLegs | TargetBodyPart.FullArms | TargetBodyPart.Chest |
                                  TargetBodyPart.Groin;

    [DataField]
    public float DisgustRate;
}

[Serializable, NetSerializable]
public enum SecondSkinKey : byte
{
    Key,
    Equipped,
    Color,
}
