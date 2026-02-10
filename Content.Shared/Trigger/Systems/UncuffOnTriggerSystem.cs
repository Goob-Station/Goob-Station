using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Shared.Trigger.Systems;

public sealed class UncuffOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!; //goob
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!; //goob
    [Dependency] private readonly SharedTransformSystem _transform = default!; //goob

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UncuffOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<UncuffOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<CuffableComponent>(target.Value, out var cuffs) || cuffs.Container.ContainedEntities.Count < 1)
            return;

        //Goob start freedom implant buff

        if (TryComp<PullableComponent>(target, out var pullable) && pullable.Puller.HasValue)
        {
            _stun.TryUpdateParalyzeDuration(pullable.Puller.Value, TimeSpan.FromSeconds(5)); // fuck it we hardcode
            args.Handled = true;
        }
        if (TryComp<EnsnareableComponent>(target, out var ensnareable) && ensnareable.Container.ContainedEntities.Count > 0)
        {
            var bola = ensnareable.Container.ContainedEntities[0];
            // Yes this is dumb, but trust me this is the best way to do this. Bola code is fucking awful.
            _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, target.Value, 0f, new EnsnareableDoAfterEvent(), target, target, bola));
            _transform.DropNextTo(bola, target.Value);
            args.Handled = true;
        }

        //Goob end freedom implant buff

        _cuffable.Uncuff(target.Value, args.User, cuffs.LastAddedCuffs);
        args.Handled = true;
    }
}
