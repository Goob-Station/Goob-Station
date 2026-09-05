
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Clumsy;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnBananaTouch(BananaTouchEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        _stun.TryUpdateParalyzeDuration(ev.Target, ev.ParalyzeDuration);
        _jitter.DoJitter(ev.Target, ev.JitterStutterDuration, true);
        _stutter.DoStutter(ev.Target, ev.JitterStutterDuration, true);

        var targetWizard = HasComp<WizardComponent>(ev.Target) || HasComp<ApprenticeComponent>(ev.Target);

        if (!targetWizard)
            EnsureComp<ClumsyComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        ev.Handled = true;
    }
}