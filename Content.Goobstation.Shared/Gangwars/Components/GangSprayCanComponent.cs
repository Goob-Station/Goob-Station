using Content.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Gangwars.Components;

/// <summary>
/// A spray can that paints a gang sign in the gang members color when used.
/// Color application gets handled by GangColorComponent which is added to the sign entity when it's spawned.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GangSprayCanComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Charges = 5;

    [DataField, AutoNetworkedField]
    public bool IsEmpty;

    [DataField]
    public int GangPoints = 100;

    [DataField]
    public ProtoId<WeightedRandomPrototype> SignPrototypes = "GangSigns";

    [DataField]
    public SoundSpecifier? UseSound = new SoundPathSpecifier("/Audio/Effects/spray2.ogg");

    [DataField]
    public SoundSpecifier? EmptySound = new SoundPathSpecifier("/Audio/_Goobstation/Items/can_crush.ogg");

    [DataField]
    public TimeSpan SprayDelay = TimeSpan.FromSeconds(5);
}
