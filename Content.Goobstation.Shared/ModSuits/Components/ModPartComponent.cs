using Content.Goobstation.Shared.ModSuits.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ModSuits.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ModSuitSystem))]
public sealed partial class ModPartComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Suit;

    [DataField, AutoNetworkedField]
    public string Slot = string.Empty;

    [DataField, AutoNetworkedField]
    public bool Sealed;

    [DataField]
    public string StoredContainerId = "mod-part-stored";

    [ViewVariables]
    public ContainerSlot StoredContainer = default!;

    [DataField]
    public LocId SealUpPopup = "modsuit-part-seal-up";

    [DataField]
    public LocId SealDownPopup = "modsuit-part-seal-down";

    [DataField]
    public SoundSpecifier SealSound = new SoundPathSpecifier("/Audio/Mecha/mechmove03.ogg");
}
