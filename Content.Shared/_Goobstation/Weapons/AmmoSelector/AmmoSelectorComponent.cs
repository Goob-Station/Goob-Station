using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AmmoSelectorComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<SelectableAmmoPrototype>> Prototypes = new();

    [DataField, AutoNetworkedField]
    public SelectableAmmoPrototype? CurrentlySelected;

    [DataField]
    public SoundSpecifier? SoundSelect = new SoundPathSpecifier("/Audio/Weapons/Guns/Misc/selector.ogg");
}
