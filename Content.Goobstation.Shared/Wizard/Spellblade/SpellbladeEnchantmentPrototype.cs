using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Spellblade;

[DataDefinition]
[Prototype("spellbladeEnchantment")]
public sealed partial class SpellbladeEnchantmentPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public object? Event;
}
