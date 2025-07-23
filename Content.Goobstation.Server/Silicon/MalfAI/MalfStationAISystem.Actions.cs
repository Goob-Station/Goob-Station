using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Goobstation.Shared.Silicon.MalfAI.Events;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Explosion.Components;
using Content.Shared.Explosion.Components.OnTrigger;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly TriggerSystem _trigger = default!;

    public void InitializeActions()
    {
        SubscribeLocalEvent<MalfStationAIComponent, MachineOverloadActionEvent>(OnOverloadAction);
    }

    private void OnOverloadAction(Entity<MalfStationAIComponent> ent, ref MachineOverloadActionEvent args)
    {
        if (HasComp<ActiveTimerTriggerComponent>(args.Target))
            return;

        if (!TryComp<ApcPowerReceiverComponent>(args.Target, out var machine))
            return;

        if (!machine.Powered)
            return;

        var ev = new OnAboutToUseCostlyAbility(ent.Comp.MachineOverloadCost);

        RaiseLocalEvent(ent, ref ev);

        if (ev.Cancelled)
            return;

        // Explosive hack begin.

        // Basically you *can* add an explosive comp to an entity BUT
        // you can't change any of the variables outside of the ExplosiveSystem SO
        // I'm storing a "default" explosive component on the action entity itself and
        // just copying that over. 

        if (!TryComp<ExplosiveComponent>(args.Action, out var explosive))
            return;

        CopyComp(args.Action, args.Target, explosive);

        // Explosive hack end.

        EnsureComp<ExplosiveComponent>(args.Target);
        EnsureComp<ExplodeOnTriggerComponent>(args.Target);

        _trigger.HandleTimerTrigger(args.Target, ent, ent.Comp.SecondsToOverload, 1.0f, 0.0f, ent.Comp.BuzzingSound);
    }
}