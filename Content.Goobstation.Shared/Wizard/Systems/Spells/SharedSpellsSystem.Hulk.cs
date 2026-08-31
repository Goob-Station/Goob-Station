using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Wizard.Mutate;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailMutateSilicon = "spell-fail-mutate-silicon";

    private void OnMutate(MutateSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<SiliconComponent>(ev.Performer))
        {
            _popup.PopupClient(Loc.GetString(_locFailMutateSilicon), ev.Performer);
            return;
        }

        EnsureComp<HulkComponent>(ev.Performer).Duration = ev.Duration;

        ev.Handled = true;
    }
}