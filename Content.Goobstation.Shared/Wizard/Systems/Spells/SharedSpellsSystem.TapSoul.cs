using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Damage;
using Content.Shared.Gibbing.Events;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locFailNoSoul = "spell-fail-no-soul";
    private LocId _locFailNoSpells = "spell-fail-no-spells";
    private LocId _locFailSoulTapDead = "spell-soul-tap-dead-message-user";
    private LocId _locMsgSoulTapAlmostDead = "spell-soul-tap-almost-dead-message";
    private LocId _locMsgSoulTap = "spell-soul-tap-message";

    private void OnSoulTap(SoulTapEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!_mind.TryGetMind(ev.Performer, out var mind, out _) || HasComp<SoulBoundComponent>(mind) ||
            _tag.HasTag(ev.Performer, ev.DeadTag))
        {
            _popup.PopupClient(Loc.GetString(_locFailNoSoul), ev.Performer);
            return;
        }

        if (!RechargeAllSpells(ev.Performer, ev.Action.Owner))
        {
            _popup.PopupClient(Loc.GetString(_locFailNoSpells), ev.Performer);
            return;
        }

        if (!TryComp(ev.Performer, out MobThresholdsComponent? thresholds))
            return;

        if (!_mobThreshold.TryGetThresholdForState(ev.Performer, MobState.Dead, out var dead, thresholds))
            return;

        ev.Handled = true;

        var targetHealth = dead.Value - ev.MaxHealthReduction;
        var kill = false;
        if (targetHealth < 1)
        {
            targetHealth = 1;
            kill = true;
        }

        if (_mobThreshold.TryGetThresholdForState(ev.Performer, MobState.Critical, out var crit, thresholds) &&
            targetHealth <= crit)
            _mobThreshold.SetMobStateThreshold(ev.Performer, targetHealth - 0.01, MobState.Critical, thresholds);

        _mobThreshold.SetMobStateThreshold(ev.Performer, targetHealth, MobState.Dead, thresholds);

        if (kill)
        {
            _tag.AddTag(ev.Performer, ev.DeadTag);

            _popup.PopupClient(Loc.GetString(_locFailSoulTapDead), ev.Performer, PopupType.LargeCaution);

            var dmg = _damageable.TryChangeDamage(ev.Performer,
                new DamageSpecifier(_protoManager.Index(ev.KillDamage), 666),
                true);
            if ((dmg == null || dmg.GetTotal() < 1) && _timing.IsFirstTimePredicted)
                _body.GibBody(ev.Performer, contents: GibContentsOption.Gib);
        }

        if (_mobState.IsDead(ev.Performer))
        {
            var message = Loc.GetString("spell-soul-tap-dead-message-others",
                ("uid", Identity.Entity(ev.Performer, EntityManager)));
            _popup.PopupEntity(message, ev.Performer, Filter.PvsExcept(ev.Performer), true, PopupType.LargeCaution);
            return;
        }

        if (TerminatingOrDeleted(ev.Performer) || EntityManager.IsQueuedForDeletion(ev.Performer))
            return;

        if (targetHealth - ev.MaxHealthReduction < 1)
            _popup.PopupClient(Loc.GetString(_locMsgSoulTapAlmostDead), ev.Performer, PopupType.LargeCaution);
        else
            _popup.PopupClient(Loc.GetString(_locMsgSoulTap), ev.Performer, PopupType.MediumCaution);
    }
}