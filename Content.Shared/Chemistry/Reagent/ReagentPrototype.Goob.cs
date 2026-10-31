using Content.Shared._ES.Fluids;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.Reagent;

public sealed partial class ReagentPrototype
{
    [DataField]
    public ProtoId<ESPuddleSpriteSetPrototype> PuddleSpriteSet = "Default";

    /// <summary>
    /// Optional override for <see cref="SubstanceColor"/> for usage only in puddles.
    /// </summary>
    [DataField]
    public Color? PuddleColor;
}
