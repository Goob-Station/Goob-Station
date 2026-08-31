using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Speech.Components;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class SharedGoobSpellsSystem
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    private void OnScreamForMe(ScreamForMeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer)
        || IsTouchSpellDenied(ev.Target))
            return;

        if (HasComp<SiliconComponent>(ev.Target))
        {
            _popup.PopupClient(Loc.GetString(_locFailSilicon), ev.Performer);
            return;
        }

        if (!TryComp(ev.Target, out BloodstreamComponent? bloodstream))
            return;

        if (TryComp(ev.Target, out VocalComponent? vocal))
            _chat.TryEmoteWithChat(ev.Target, vocal.ScreamId);

        Spawn(ev.Effect, _xform.GetMapCoordinates(ev.Target));

        _bloodstream.SpillAllSolutions((ev.Target, bloodstream));
        _bloodstream.TryModifyBleedAmount((ev.Target, bloodstream), bloodstream.MaxBleedAmount);
        EnsureComp<BloodlossDamageMultiplierComponent>(ev.Target);

        ev.Handled = true;
    }
}