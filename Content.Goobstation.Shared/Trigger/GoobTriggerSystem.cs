using Content.Shared.DoAfter;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Content.Shared.Trigger;

namespace Content.Goobstation.Shared.Trigger;

public sealed class GoobTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedEnsnareableSystem _snare = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParalyzePullerOnTriggerComponent, TriggerEvent>(OnParalyzeTrigger);
        SubscribeLocalEvent<RemoveSnareOnTriggerComponent, TriggerEvent>(OnSnareTrigger);
    }

    private void OnSnareTrigger(Entity<RemoveSnareOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<EnsnareableComponent>(target, out var ensnareable) || !ensnareable.IsEnsnared ||
            ensnareable.Container.ContainedEntities.Count <= 0)
            return;

        var bola = ensnareable.Container.ContainedEntities[0];
        _snare.ForceFree(bola, Comp<EnsnaringComponent>(bola));
        args.Handled = true;
    }

    private void OnParalyzeTrigger(Entity<ParalyzePullerOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<PullableComponent>(target, out var pullable) || !pullable.Puller.HasValue)
            return;

        _stun.TryUpdateParalyzeDuration(pullable.Puller.Value, TimeSpan.FromSeconds(5));
        args.Handled = true;
    }
}
