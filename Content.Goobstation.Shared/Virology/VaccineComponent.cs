using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class VaccineComponent : Component
{
    [ViewVariables]
    public int? Genotype;

    [ViewVariables]
    public bool Used = false;

    [DataField]
    public SoundSpecifier InjectSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");
};
