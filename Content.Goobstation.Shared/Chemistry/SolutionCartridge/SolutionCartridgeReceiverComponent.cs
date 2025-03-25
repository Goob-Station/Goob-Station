using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Chemistry.SolutionCartridge;

[RegisterComponent, NetworkedComponent]
public sealed partial class SolutionCartridgeReceiverComponent : Component
{
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public SoundSpecifier InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");
}
