using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Funkystation.WallStains;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;

namespace Content.Shared.Body.Systems;

public abstract partial class SharedBloodstreamSystem
{
    private const float WallSplatterMinDamage = 2f;
    private const float WallSplatterPerDamage = 0.15f;

    private void SplatterBloodOnWalls(Entity<BloodstreamComponent> ent, float damage)
    {
        if (damage < WallSplatterMinDamage
            || !SolutionContainer.ResolveSolution(ent.Owner, ent.Comp.BloodSolutionName, ref ent.Comp.BloodSolution, out var blood)
            || blood.Volume <= 0)
            return;

        var amount = FixedPoint2.Min(FixedPoint2.New(damage * WallSplatterPerDamage), blood.Volume);
        if (amount <= 0)
            return;

        var splatter = blood.Clone().SplitSolution(amount);
        RaiseSplashOnWall(Transform(ent.Owner).Coordinates, splatter);
    }

    private void RaiseSplashOnWall(Robust.Shared.Map.EntityCoordinates coordinates, Solution solution)
    {
        var ev = new SplashOnWallEvent(coordinates, solution);
        RaiseLocalEvent(ref ev);
    }
}
