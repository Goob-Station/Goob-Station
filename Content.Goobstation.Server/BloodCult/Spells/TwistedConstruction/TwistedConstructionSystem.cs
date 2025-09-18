using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Stack;
using Content.Shared.DoAfter;
using Content.Shared.Stacks;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.BloodCult.Spells.TwistedConstruction;

public sealed class TwistedConstructionSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<Goobstation.Shared.BloodCult.Spells.BloodCultTwistedConstructionEvent>(OnTwistedConstruction);
        SubscribeLocalEvent<Goobstation.Shared.BloodCult.Components.TwistedConstructionTargetComponent, Goobstation.Shared.BloodCult.Spells.TwistedConstructionDoAfterEvent>(OnDoAfter);
    }

    private void OnTwistedConstruction(Goobstation.Shared.BloodCult.Spells.BloodCultTwistedConstructionEvent ev)
    {
        if (ev.Handled || !TryComp(ev.Target, out Goobstation.Shared.BloodCult.Components.TwistedConstructionTargetComponent? twistedConstruction))
            return;

        var args = new DoAfterArgs(EntityManager,
            ev.Performer,
            twistedConstruction.DoAfterDelay,
            new Goobstation.Shared.BloodCult.Spells.TwistedConstructionDoAfterEvent(),
            ev.Target);

        if (_doAfter.TryStartDoAfter(args))
            ev.Handled = true;
    }

    private void OnDoAfter(Entity<Goobstation.Shared.BloodCult.Components.TwistedConstructionTargetComponent> target, ref Goobstation.Shared.BloodCult.Spells.TwistedConstructionDoAfterEvent args)
    {
        var replacement = Spawn(target.Comp.ReplacementProto, _transform.GetMapCoordinates(target));
        if (TryComp(target, out StackComponent? stack) && TryComp(replacement, out StackComponent? targetStack))
            _stack.SetCount(replacement, stack.Count, targetStack);

        if (_mind.TryGetMind(target, out var mindId, out _))
            _mind.TransferTo(mindId, replacement);

        QueueDel(target);
    }
}
