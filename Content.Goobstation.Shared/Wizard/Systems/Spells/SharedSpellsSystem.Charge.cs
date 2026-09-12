using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._DV.Carrying;
using Content.Shared.Charges.Components;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailNoSpellsToChargePulled = "spell-charge-no-spells-to-charge-pulled";
    private LocId _locMsgSpellsRecharged = "spell-charge-spells-charged-entity";
    private LocId _locMsgSpellsRechargedPulled = "spell-charge-spells-charged-pulled";

    private bool ChargeItem(EntityUid uid, ChargeMagicEvent ev)
    {
        if (!TryComp(uid, out BatteryComponent? battery) || battery.LastCharge >= battery.MaxCharge)
            return false;

        if (_tag.HasTag(uid, ev.WandTag))
        {
            var difference = battery.MaxCharge - battery.LastCharge;
            var charge = MathF.Min(difference, ev.WandChargeRate);
            var degrade = charge * ev.WandDegradePercentagePerCharge;
            var afterDegrade = MathF.Max(ev.MinWandDegradeCharge, battery.MaxCharge - degrade);
            if (battery.MaxCharge > ev.MinWandDegradeCharge)
                _battery.SetMaxCharge(uid, afterDegrade);
            _battery.SetCharge(uid, battery.LastCharge + charge);
        }
        else
            _battery.SetCharge(uid, battery.MaxCharge);

        PopupCharged(uid, ev.Performer);
        return true;
    }

    protected virtual void ChargeEffectRelay(EntityUid performer) { }

    protected void PopupCharged(EntityUid uid, EntityUid performer)
    {
        var message = Loc.GetString(_locMsgSpellsRecharged,
            ("entity", Identity.Entity(uid, EntityManager)));

        _popup.PopupPredicted(message, performer, null, PopupType.Medium);
    }

    private void OnCharge(ChargeMagicEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        ev.Handled = true;

        // clientside shit
        ChargeEffectRelay(ev.Performer);

        if (TryComp<PullerComponent>(ev.Performer, out var puller)
        && HasComp<PullableComponent>(puller.Pulling)
        && RechargePerson(puller.Pulling.Value))
            return;

        if (TryComp(ev.Performer, out CarryingComponent? carrying)
        && RechargePerson(carrying.Carried))
            return;

        if (!TryComp(ev.Performer, out HandsComponent? hands))
            return;

        foreach (var item in _hands.EnumerateHeld((ev.Performer, hands)))
        {
            if (_tag.HasAnyTag(item, ev.RechargeTags))
            {
                if (TryComp<LimitedChargesComponent>(item, out var limitedCharges))
                {
                    _charges.SetCharges((item, limitedCharges), limitedCharges.MaxCharges);
                    PopupCharged(item, ev.Performer);
                    break;
                }

                if (TryComp<BasicEntityAmmoProviderComponent>(item, out var basicAmmoComp) &&
                    basicAmmoComp is { Count: not null, Capacity: not null } &&
                    basicAmmoComp.Count < basicAmmoComp.Capacity)
                {
                    _gun.UpdateBasicEntityAmmoCount((item, basicAmmoComp), basicAmmoComp.Capacity.Value);
                    PopupCharged(item, ev.Performer);
                    break;
                }
            }

            if (ChargeItem(item, ev))
                break;
        }

        return;

        bool RechargePerson(EntityUid uid)
        {
            if (RechargeAllSpells(uid))
            {
                PopupCharged(uid, ev.Performer);
                _popup.PopupPredicted(Loc.GetString(_locMsgSpellsRechargedPulled), uid, uid, PopupType.Medium);
                ev.Handled = true;
                return true;
            }

            _popup.PopupPredicted(Loc.GetString(_locFailNoSpellsToChargePulled), uid, uid, PopupType.Medium);
            return false;
        }
    }
}