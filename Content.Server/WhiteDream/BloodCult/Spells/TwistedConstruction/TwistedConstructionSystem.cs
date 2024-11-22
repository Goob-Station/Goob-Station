using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Stack;
using Content.Shared.DoAfter;
using Content.Shared.Stacks;
using Content.Shared.WhiteDream.BloodCult.Components;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Spells.TwistedConstruction;

public sealed class TwistedConstructionSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly StackSystem _stack = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultTwistedConstructionEvent>(OnTwistedConstruction);
        SubscribeLocalEvent<TwistedConstructionTargetComponent, TwistedConstructionDoAfterEvent>(OnDoAfter);
    }

    private void OnTwistedConstruction(BloodCultTwistedConstructionEvent ev)
    {
        if (ev.Handled || !TryComp(ev.Target, out TwistedConstructionTargetComponent? twistedConstruction))
            return;

        var args = new DoAfterArgs(EntityManager,
            ev.Performer,
            twistedConstruction.DoAfterDelay,
            new TwistedConstructionDoAfterEvent(),
            ev.Target);

        if (_doAfter.TryStartDoAfter(args))
            ev.Handled = true;
    }

    private void OnDoAfter(Entity<TwistedConstructionTargetComponent> target, ref TwistedConstructionDoAfterEvent args)
    {
        var replacement = Spawn(target.Comp.ReplacementProto, _transform.GetMapCoordinates(target));
        if (TryComp(target, out StackComponent? stack) && TryComp(replacement, out StackComponent? targetStack))
            _stack.SetCount(replacement, stack.Count, targetStack);

        if (_mind.TryGetMind(target, out var mindId, out _))
            _mind.TransferTo(mindId, replacement);

        QueueDel(target);
    }
}
