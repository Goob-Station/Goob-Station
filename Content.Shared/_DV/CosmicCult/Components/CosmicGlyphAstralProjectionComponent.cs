using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicGlyphAstralProjectionComponent : Component
{
    [DataField]
    public EntProtoId SpawnProjection = "MobCosmicAstralProjection";

    /// <summary>
    /// The duration of the astral projection
    /// </summary>
    [DataField]
    public TimeSpan AstralDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public DamageSpecifier ProjectionDamage = new()
    {
        DamageDict = new() {
            { "Asphyxiation", 20 }
        }
    };
}
