using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Abilities.Mime;
using Content.Shared.Speech.Muting;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedGoobSpellsSystem
{
    private void OnMimeMalaise(MimeMalaiseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        _stun.TryUpdateParalyzeDuration(ev.Target, ev.ParalyzeDuration);

        var targetWizard = HasComp<WizardComponent>(ev.Target) || HasComp<ApprenticeComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        if (!targetWizard)
        {
            var powers = EnsureComp<MimePowersComponent>(ev.Target);
            powers.CanBreakVow = false;
            Dirty(ev.Target, powers);
        }
        else
        {
            _statusEffects.TryAddStatusEffect<MutedComponent>(ev.Target, "Muted", ev.WizardMuteDuration, true);
        }

        ev.Handled = true;
    }
}