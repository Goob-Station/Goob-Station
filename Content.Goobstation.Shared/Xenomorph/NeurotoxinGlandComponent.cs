using Content.Goobstation.Maths.FixedPoint; // Goobstation
using Robust.Shared.Audio; // Goobstation
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes; // Goobstation

namespace Content.Goobstation.Shared.Xenomorph;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NeurotoxinGlandComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active = false;

    /// <summary>
    /// What action.
    /// </summary>
    [DataField]
    public EntProtoId ActionId = "ActionAcidSpit";

    /// <summary>
    /// The plasma cost per shot.
    /// </summary>
    [DataField]
    public FixedPoint2 FireCost = FixedPoint2.New(20);

    /// <summary>
    /// The projectile prototype to spawn.
    /// </summary>
    [DataField]
    public EntProtoId Proto = "BulletNeurotoxin";

    /// <summary>
    /// The fire rate of the gun.
    /// </summary>
    [DataField]
    public float FireRate = 0.60f;

    /// <summary>
    /// The sound played when shooting.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundGunshot = new SoundPathSpecifier("/Audio/Weapons/Xeno/alien_spitacid.ogg");

    /// <summary>
    /// Set to null to play no sound.
    /// It wouldn't make sense for a xenomorph to make a clicking sound.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundEmpty = null;
}
