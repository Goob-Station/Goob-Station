using Content.Shared._Trauma.EntityEffects;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityEffects;

namespace Content.Trauma.Shared.EntityEffects;

/// <summary>
/// Applies effects to a solution entity of a given name.
/// </summary>
public sealed partial class RelaySolution : EntityEffectBase<RelaySolution>
{
    /// <summary>
    /// The solution to get.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// Effects to apply to the solution entity.
    /// </summary>
    [DataField(required: true)]
    public EntityEffect[] Effects = default!;
}

public sealed partial class RelaySolutionEffectSystem : EntityEffectSystem<SolutionContainerManagerComponent, RelaySolution>
{
    [Dependency] private EffectDataSystem _data = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    protected override void Effect(Entity<SolutionContainerManagerComponent> ent, ref EntityEffectEvent<RelaySolution> args)
    {
        if (!_solution.TryGetSolution(ent.AsNullable(), args.Effect.Name, out var solution, out _, true))
            return;

        var uid = solution.Value.Owner;
        _data.CopyData(ent, uid);
        _effects.ApplyEffects(uid, args.Effect.Effects, args.Scale, args.User);
        _data.ClearData(uid);
    }
}
