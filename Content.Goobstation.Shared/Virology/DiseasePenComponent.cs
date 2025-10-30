using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class DiseasePenComponent : Component
{
    [ViewVariables]
    public int? Genotype;

    [ViewVariables]
    public EntityUid? DiseaseUid;

    [ViewVariables]
    public bool Used = false;

    [DataField, ViewVariables]
    public SoundSpecifier InjectSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");

    [DataField]
    public bool Vaccine = true;

    [DataField]
    public TimeSpan InjectTime = TimeSpan.FromSeconds(8);
};
