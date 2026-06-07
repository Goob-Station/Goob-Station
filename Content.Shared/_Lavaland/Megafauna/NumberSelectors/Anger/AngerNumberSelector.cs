using System.Numerics;
using Content.Shared._Lavaland.Anger.Systems;

namespace Content.Shared._Lavaland.Megafauna.NumberSelectors;

/// <summary>
/// Scales the number based on current anger percentage.
/// </summary>
public sealed partial class AngerNumberSelector : MegafaunaNumberSelector
{
    [DataField]
    public Vector2 Range = new(1f, 1f);

    /// <summary>
    /// If true, will inverse the calculation so the value will
    /// become smaller with bigger aggression.
    /// </summary>
    [DataField]
    public bool Inverse;

    public override float Get(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.Entity;
        var angerSystem = entMan.System<AngerSystem>();
        return angerSystem.GetAngerScale(uid, Range.X, Range.Y, Inverse);
    }
}
