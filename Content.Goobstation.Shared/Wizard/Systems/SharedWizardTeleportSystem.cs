// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Magic;
using Content.Shared.UserInterface;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class WizardTeleportEvent : InstantActionEvent;

public abstract class SharedWizardTeleportSystem : EntitySystem
{
    [Dependency] private readonly SharedMagicSystem _magic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeleportScrollComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TeleportScrollComponent, ActivatableUIOpenAttemptEvent>(OnUiOpenAttempt);
        SubscribeLocalEvent<WizardTeleportEvent>(OnTeleport);
    }

    public virtual void OnTeleportSpell(EntityUid performer, EntityUid action) { }

    private void OnUiOpenAttempt(Entity<TeleportScrollComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (ent.Comp.UsesLeft <= 0)
            args.Cancel();
    }

    private void OnExamined(Entity<TeleportScrollComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("teleport-scroll-uses-left", ("uses", ent.Comp.UsesLeft)));
    }

    private void OnTeleport(WizardTeleportEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        OnTeleportSpell(ev.Performer, ev.Action);
    }
}