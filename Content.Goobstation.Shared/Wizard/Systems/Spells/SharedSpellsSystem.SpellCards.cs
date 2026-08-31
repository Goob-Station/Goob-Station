using System.Numerics;
using Content.Goobstation.Common.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnSpellCards(SpellCardsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!ValidateLockOnAction(ev))
            return;

        if (!TryComp(ev.Action.Owner, out SpellCardsActionComponent? spellCardsAction))
            return;

        ShootSpellCardsRelay(ev, spellCardsAction.PurpleCard ? ev.PurpleProto : ev.RedProto);

        spellCardsAction.PurpleCard = !spellCardsAction.PurpleCard;

        ev.Handled = true;
        if (_net.IsClient)
            return;
        spellCardsAction.UsesLeft--;
        if (spellCardsAction.UsesLeft > 0)
            _actions.SetUseDelay(ev.Action.Owner, TimeSpan.FromSeconds(0.5));
        else
        {
            _actions.SetUseDelay(ev.Action.Owner, spellCardsAction.UseDelay);
            spellCardsAction.UsesLeft = spellCardsAction.CastAmount;
            RaiseNetworkEvent(new StopTargetingEvent(), ev.Performer);
        }
    }

    private bool ValidateLockOnAction(WorldTargetActionEvent ev)
    {
        if (!TryComp(ev.Action.Owner, out LockOnMarkActionComponent? lockOnMark))
            return false;

        if (!_xformQuery.TryComp(ev.Entity, out var xform))
            return true;

        if (!HasComp<MobStateComponent>(ev.Entity.Value) || !HasComp<DamageableComponent>(ev.Entity.Value))
            return false;

        return _xform.InRange(ev.Target, xform.Coordinates, lockOnMark.LockOnRadius + 1f);
    }

    // TODO: predict
    protected virtual void ShootSpellCardsRelay(SpellCardsEvent ev, EntProtoId proto) { }
}