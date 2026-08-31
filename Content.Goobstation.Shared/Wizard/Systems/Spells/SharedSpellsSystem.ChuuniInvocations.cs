using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard.Chuuni;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailCantWear = "spell-fail-cant-wear-eyepatch";
    private LocId _locFailAlreadyWear = "spell-fail-already-wear-eyepatch";

    private void OnChuuniInvocations(ChuuniInvocationsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!_inventory.HasSlot(ev.Performer, "eyes"))
        {
            _popup.PopupClient(Loc.GetString(_locFailCantWear), ev.Performer);
            return;
        }

        if (_inventory.TryGetSlotEntity(ev.Performer, "eyes", out var eyePatch) &&
            HasComp<ChuuniEyepatchComponent>(eyePatch))
        {
            _popup.PopupClient(Loc.GetString(_locFailAlreadyWear), ev.Performer);
            return;
        }

        SetGear(ev.Performer, ev.Gear);

        if (_inventory.TryGetSlotEntity(ev.Performer, "head", out var hat)
            && _tag.HasTag(hat.Value, ev.WizardHatTag))
            PredictedQueueDel(hat);

        ev.Handled = true;
    }
}