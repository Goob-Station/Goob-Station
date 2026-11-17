using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts;


[Prototype]
public sealed partial class MartialArtPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<ProtoId<MartialArtsComboPrototype>> Combos;

    // todo cleaner yamley way to handle intent modifiers
    [DataField]
    public float FistDamageMultiplier = 1;

    [DataField]
    public float FistStaminaDamage = 0;

    [DataField]
    public ComponentRegistry ComponentGrant = [];

    [DataField(required: true)]
    public float Weight = 0;
}
