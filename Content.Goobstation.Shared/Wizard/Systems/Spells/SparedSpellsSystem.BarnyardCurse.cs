using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Chat;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailCantWearMask = "spell-fail-target-cant-wear-mask";
    private LocId _locFailAlreadyCursed = "spell-fail-target-cursed";

    private void OnBarnyardCurse(BarnyardCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (IsTouchSpellDenied(ev.Target))
        {
            ev.Handled = true;
            return;
        }

        if (ev.Masks.Count == 0)
            return;

        if (!TryComp(ev.Target, out InventoryComponent? inventory))
            return;

        if (!_inventory.HasSlot(ev.Target, "mask", inventory))
        {
            _popup.PopupClient(Loc.GetString(_locFailAlreadyCursed), ev.Performer);
            return;
        }

        if (_inventory.TryGetSlotEntity(ev.Target, "mask", out var ent, inventory) &&
            HasComp<UnremoveableComponent>(ent) && _tag.HasTag(ent.Value, ev.CursedMaskTag))
        {
            _popup.PopupClient(Loc.GetString(_locFailAlreadyCursed), ev.Performer);
            return;
        }

        if (_net.IsClient)
            return;

        var (maskEnt, sound) = _random.Pick(ev.Masks);

        var gear = new Dictionary<string, EntProtoId>
        {
            { "mask", maskEnt },
        };

        SetGear(ev.Target, gear, inventoryComponent: inventory);

        if (sound != null)
            _audio.PlayEntity(sound, Filter.Pvs(ev.Target), ev.Target, true);

        // This should transform into animal noise
        _chat.TrySendInGameICMessage(ev.Target, "!", InGameICChatType.Speak, true);

        ev.Handled = true;
    }
}