using Content.Shared._Goobstation.Wizard;
using Content.Shared.StatusIcon.Components;

namespace Content.Client._Goobstation.Wizard;

public sealed class SpellsSystem : SharedSpellsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardComponent, GetStatusIconsEvent>(GetWizardIcon);
    }

    private void GetWizardIcon(Entity<WizardComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ProtoMan.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
