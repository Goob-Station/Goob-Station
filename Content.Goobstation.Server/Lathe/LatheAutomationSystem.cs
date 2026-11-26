using Content.Goobstation.Shared.Factory;
using Content.Server.Lathe;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Lathe;

namespace Content.Goobstation.Server.Lathe;

public sealed partial class LatheAutomationSystem : EntitySystem
{
    [Dependency] private AutomationSystem _automation = default!;
    [Dependency] private LatheSystem _lathe = default!;

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

        _lathe.TryAddToQueue(ent.Owner, recipe, 1);
        _lathe.TryStartProducing(ent.Owner);
    }

}
