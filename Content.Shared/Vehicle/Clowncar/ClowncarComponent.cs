//using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Actions;

namespace Content.Shared.Vehicle.Clowncar;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedClowncarSystem))]
public sealed partial class ClowncarComponent : Component
{
    [DataField("thankRiderAction")]
    [ViewVariables]
    public string ThankRiderAction = "ActionThankDriver";

    [DataField("thankCounter")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int ThankCounter;

    #region Cannon
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? CannonEntity = default!;

    [DataField("cannonPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [ViewVariables(VVAccess.ReadWrite)]
    public string CannonPrototype = "ClowncarCannon";

    /*[DataField("cannonAction")]
    [ViewVariables]
    public InstantAction? CannonAction;*/

    [DataField("cannonSetupDelay")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan CannonSetupDelay = TimeSpan.FromSeconds(2);

    [DataField("cannonRange")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float CannonRange = 30; // seems to mutch

    /// <summary>
    /// Time the people we shoot out of the cannon and the person they
    /// collide with get paralyzed for
    /// </summary>
    [DataField("shootingParalyzeTime")]
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ShootingParalyzeTime = TimeSpan.FromSeconds(5);

    #endregion

    #region Sound
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("cannonActivateSound")]
    public SoundSpecifier CannonActivateSound = new SoundPathSpecifier("/Audio/Effects/Vehicle/Clowncar/clowncar_activate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("cannonDeactivateSound")]
    public SoundSpecifier CannonDeactivateSound = new SoundPathSpecifier("/Audio/Effects/Vehicle/Clowncar/clowncar_deactivate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("fartSound")]
    public SoundSpecifier FartSound = new SoundPathSpecifier("/Audio/Effects/Vehicle/Clowncar/clowncar_fart.ogg");

    #endregion

}

//public sealed partial class ThankRiderAction : InstantActionEvent { }
public sealed partial class CannonAction : InstantActionEvent { }
