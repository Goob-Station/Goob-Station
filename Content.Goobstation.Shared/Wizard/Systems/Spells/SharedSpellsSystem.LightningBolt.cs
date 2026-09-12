using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Interaction;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailLightningBoltNoRange = "spell-fail-lightning-bolt";

    private void OnLightningBolt(LightningBoltEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (!_examine.InRangeUnOccluded(ev.Performer, ev.Target, SharedInteractionSystem.MaxRaycastRange))
        {
            _popup.PopupClient(Loc.GetString(_locFailLightningBoltNoRange), ev.Performer);
            return;
        }

        _teslaBlast.ShootLightning(ev.Performer, ev.Target, ev.Proto, ev.Damage);

        ev.Handled = true;
    }
}