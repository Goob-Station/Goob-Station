using Content.Shared._Lavaland.MobPhases;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

/// <summary>
/// This condition is used to inform the selector on which boss phases the actions can be used.
/// You can either use Phases [ numberX, numberY, etc] or use minPhase: numberX maxphase: numberY to inform on which phases it can be used.
/// it is an either or scenario, realistically using Phases is more practical, however if your boss has lots of phases for any given reason, using MinMax might be better.
/// </summary>
public sealed partial class MegafaunaPhaseCondition : MegafaunaCondition
{
    /// <summary>
    /// Allowed phases. Empty = always allowed.
    /// </summary>
    [DataField]
    public List<int> Phases = new();

    /// <summary>
    /// The minimum phase number needed for this action to be picked.
    /// </summary>
    [DataField]
    public int? MinPhase;

    /// <summary>
    /// The maximum phase number allowed for this action to be picked.
    /// </summary>
    [DataField]
    public int? MaxPhase;

    public override bool EvaluateImplementation(MegafaunaCalculationBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<MobPhasesComponent>(
                args.Entity, out var phases))
            return false;

        var current = phases.CurrentPhase;

        if (Phases.Count > 0 && !Phases.Contains(current))
            return false;

        if (MinPhase.HasValue && current < MinPhase.Value)
            return false;

        if (MaxPhase.HasValue && current > MaxPhase.Value)
            return false;

        return true;
    }
}
