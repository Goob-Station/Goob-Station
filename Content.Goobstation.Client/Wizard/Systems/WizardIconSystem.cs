using Content.Shared._Goobstation.Wizard;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Wizard.Systems;

public sealed partial class WizardSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardComponent, GetStatusIconsEvent>(GetWizardIcon);
        SubscribeLocalEvent<ApprenticeComponent, GetStatusIconsEvent>(GetApprenticeIcon);
    }

    private void GetWizardIcon(Entity<WizardComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_protoManager.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetApprenticeIcon(Entity<ApprenticeComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_protoManager.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}