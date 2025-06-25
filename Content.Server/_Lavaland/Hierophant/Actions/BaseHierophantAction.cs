using Content.Server._Lavaland.Megafauna;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Hierophant.Actions;

[MeansImplicitUse]
public abstract partial class BaseHierophantAction : MegafaunaAction
{
    [DataField]
    public EntProtoId DamageTile = "LavalandHierophantSquare";

    /// <summary>
    /// Controls the speed of consecutive hierophant attacks.
    /// </summary>
    [DataField]
    public float TileDamageDelay = 0.7f;
}
