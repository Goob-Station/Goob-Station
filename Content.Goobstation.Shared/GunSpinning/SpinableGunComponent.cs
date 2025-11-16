using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.GunSpinning;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class SpinableGunComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SpinTime = 6f;

    [DataField, AutoNetworkedField]
    public float FailChance = 0.2f;

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundSpin = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Guns/Spin/gun_spin.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundFail = new SoundPathSpecifier("/Audio/_Goobstation/Items/weapons/Revolver/collide1.ogg");

    [DataField, AutoNetworkedField]
    public EntityUid? SoundEntity;

}