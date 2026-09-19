using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Weapons.Melee;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem : EntitySystem
{
    private LocId _locFailNoItem = "spell-fail-sanguine-strike-no-item";
    private LocId _locFailFakeWeapon = "spell-fail-sanguine-strike-not-weapon";
    private LocId _locFailAlreadyEmpowered = "spell-fail-sanguine-strike-already-empowered";

    private void OnExsangunatingStrike(ExsanguinatingStrikeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!_hands.TryGetActiveItem(ev.Performer, out var held))
            return;

        if (!HasComp<ItemComponent>(held))
        {
            _popup.PopupClient(Loc.GetString(_locFailNoItem), ev.Performer);
            return;
        }

        if (HasComp<VirtualItemComponent>(held))
            return;

        if (HasComp<SanguineStrikeComponent>(held))
        {
            _popup.PopupClient(Loc.GetString(_locFailAlreadyEmpowered), ev.Performer);
            return;
        }

        if (!TryComp(held, out MeleeWeaponComponent? weapon) || weapon.Damage.GetTotal() == FixedPoint2.Zero)
        {
            _popup.PopupClient(Loc.GetString(_locFailFakeWeapon, ("item", held)), ev.Performer);
            return;
        }

        AddComp<SanguineStrikeComponent>(held.Value);

        ev.Handled = true;
    }
}