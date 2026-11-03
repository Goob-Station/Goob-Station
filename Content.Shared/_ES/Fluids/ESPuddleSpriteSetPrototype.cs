using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._ES.Fluids;

[Prototype("esPuddleSpriteSet")]
public sealed partial class ESPuddleSpriteSetPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Color BaseColor = Color.White;

    [DataField]
    public bool Recolor = true;

    [DataField]
    public List<SpriteSpecifier> SmallSprites = new();

    [DataField]
    public List<SpriteSpecifier> MediumSprites = new();

    [DataField]
    public List<SpriteSpecifier> LargeSprites = new();
}
