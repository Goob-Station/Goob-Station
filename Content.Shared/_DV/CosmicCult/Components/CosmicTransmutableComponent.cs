using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Indicates that an entity will be transmuted to the given prototype by using a specific glyph
/// </summary>
[RegisterComponent]
public sealed partial class CosmicTransmutableComponent : Component
{
    [DataField(required: true)]
    public EntProtoId TransmutesTo;

    [DataField(required: true)]
    public EntProtoId RequiredGlyphType;
}
