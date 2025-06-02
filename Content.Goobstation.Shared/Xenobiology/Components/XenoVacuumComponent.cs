using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenoVacuumComponent : Component
{
    /// <summary>
    /// Whether the vacuum is emagged.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsEmagged;

    /// <summary>
    /// The ID of the tank's container.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string StorageID = "tankbase";

    /// <summary>
    /// The tank's tag.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public ProtoId<TagPrototype> XenoTankTag = "XenoNozzleBackTank";

    /// <summary>
    /// The sound played when the vacuum is used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Effects/zzzt.ogg");

    /// <summary>
    /// The sound played when the tank is cleared.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? ClearSound = new SoundPathSpecifier("/Audio/Effects/trashbag3.ogg");
}
