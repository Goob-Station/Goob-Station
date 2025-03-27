//using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Actions;

namespace Content.Shared._Goobstation.Vehicles.Clowncar;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedClowncarSystem))]
public sealed partial class ClowncarComponent : Component
{
    [DataField]
    [ViewVariables] //EntProtoId
    public string Container = "clowncar_container";

    [DataField]
    [ViewVariables]
    public EntProtoId ThankRiderAction = "ActionThankDriver";

    [DataField]
    [ViewVariables]
    public EntProtoId QuietInTheBackAction = "ActionQuietBackThere";

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public int ThankCounter;

    #region Sound
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier CannonActivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_activate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier CannonDeactivateSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_deactivate_cannon.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier FartSound = new SoundPathSpecifier("/Audio/_Goobstation/Vehicle/Clowncar/clowncar_fart.ogg");

    #endregion

}

//public sealed partial class ThankRiderAction : InstantActionEvent { }
public sealed partial class CannonAction : InstantActionEvent { }
