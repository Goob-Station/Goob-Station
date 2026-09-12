using Content.Goobstation.Common.Actions;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Magic.Components;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Server.Wizard.Spells.Systems;

public sealed partial class SpellsSystem
{
    protected override bool PolymorphRelay(PolymorphSpellEvent ev)
    {
        if (ev.ProtoId == null)
            return false;

        var newEnt = _polymorph.PolymorphEntity(ev.Performer, ev.ProtoId.Value);

        if (newEnt == null)
            return false;

        if (ev.MakeWizard)
        {
            if (HasComp<WizardComponent>(ev.Performer))
                EnsureComp<WizardComponent>(newEnt.Value);
            if (HasComp<ApprenticeComponent>(ev.Performer))
                EnsureComp<ApprenticeComponent>(newEnt.Value);
        }

        _audio.PlayPvs(ev.Sound, newEnt.Value);

        var school = MagicSchool.Transmutation;
        if (TryComp(ev.Action.Owner, out MagicComponent? magic))
            school = magic.School;

        if (ev.LoadActions)
            RaiseNetworkEvent(new LoadActionsEvent(GetNetEntity(ev.Performer)), newEnt.Value);

        if (TryComp(ev.Action.Owner, out SpeakOnActionComponent? speak))
        {
            DelayedSpeech(speak.Sentence == null ? null : Loc.GetString(speak.Sentence.Value),
                newEnt.Value,
                ev.Performer,
                school);
        }

        return true;
    }
}