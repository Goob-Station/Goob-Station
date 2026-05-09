using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

/// <summary>
/// Used for those stupid chickens that make me do more work.
/// </summary>
public sealed class SpecialEggsSystem : EntitySystem
{
    [Dependency] private readonly SharedStackSystem _stack = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlateableChickenComponent, InteractUsingEvent>(OnInteract);

        SubscribeLocalEvent<CopyInjectedReagentsComponent, SolutionContainerChangedEvent>(OnChanged);
    }

    private void OnChanged(Entity<CopyInjectedReagentsComponent> ent, ref SolutionContainerChangedEvent args)
    {
        if (args.SolutionId == "Blood" || TerminatingOrDeleted(ent.Owner))
            return;

        var regen = EnsureComp<SolutionRegenerationComponent>(ent.Owner);
        regen.SolutionName = ent.Comp.Solution;
        regen.Generated = args.Solution;
        RemComp<CopyInjectedReagentsComponent>(ent.Owner);
    }

    private void OnInteract(Entity<PlateableChickenComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<PlateableChickenOreComponent>(args.Used, out var ore))
            return;

        EntityManager.AddComponents(ent.Owner, ore.Components);
        RemComp<PlateableChickenComponent>(ent.Owner);

        if (_stack.GetCount(args.Used) > 0)
        {
            _stack.ReduceCount(args.Used, 1);
            return;
        }

        Del(args.Used);
    }
}
