using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[RegisterComponent, NetworkedComponent]
public sealed partial class AmmoSelectorComponent : Component
{
    [DataField]
    public HashSet<ProtoId<SelectableAmmoPrototype>> Prototypes = new();

    [DataField]
    public SoundSpecifier? SoundSelect = new SoundPathSpecifier("/Audio/Weapons/Guns/Misc/selector.ogg");
}
