using Content.Goobstation.Shared.Factory;
using Content.Server.Lathe;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Lathe;

namespace Content.Goobstation.Server.Lathe;

public sealed class LatheAutomationSystem : EntitySystem
{
    [Dependency] private readonly AutomationSystem _automation = default!;
    [Dependency] private readonly LatheSystem _lathe = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LatheAutomationComponent, LatheStartPrintingEvent>(OnStartPrinting);
        SubscribeLocalEvent<LatheAutomationComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnStartPrinting(Entity<LatheAutomationComponent> ent, ref LatheStartPrintingEvent args)
    {
        ent.Comp.LastRecipe = args.Recipe;
    }

    private void OnSignalReceived(Entity<LatheAutomationComponent> ent, ref SignalReceivedEvent args)
    {
        if (!_automation.IsAutomated(ent))
            return;

        if (args.Port != ent.Comp.PrintPort)
            return;

        if (ent.Comp.LastRecipe is not {} recipe)
            return;

        if (args.Data is not { Count: > 0 } )
            return;
        var key = args.Data.Keys;
        var empty = 1;
        var values = args.Data.Values;
        var empty2 = 2;


        _lathe.TryAddToQueue(ent.Owner, recipe, 1);
        _lathe.TryStartProducing(ent.Owner);
    }

}
