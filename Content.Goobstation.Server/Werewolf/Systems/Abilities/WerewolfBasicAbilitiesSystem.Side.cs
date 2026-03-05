using System.Linq;
using Content.Goobstation.Server.Werewolf.Components;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Werewolf.Abilities;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Server.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Werewolf.Systems.Abilities;

/// <summary>
/// Handles side abilities and helpers
/// </summary>
public partial class WerewolfBasicAbilitiesSystem
{

    public void InitializeWerewolfSide()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfDevour>(TryDevour);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, WerewolfDevourDoAfterEvent>(DoDevour);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfGut>(TryGut);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, WerewolfGutDoAfterEvent>(DoGut);
    }
    # region devour
    private void TryDevour(EntityUid uid, WerewolfBasicAbilitiesComponent component, EventWerewolfDevour args)
    {
        if (component.Transfurmed != true)
        {
            _popup.PopupEntity(Loc.GetString("werewolf-action-fail-transfurmed"), uid, uid);
            return;
        }
        var target = args.Target;

        if (HasComp<WerewolfBitComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("werewolf-devour-fail-devoured"), uid, uid);
            return;
        }
        if (!HasComp<AbsorbableComponent>(target)) // i mean... it works? also less wizden files changes
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), uid, uid);
            return;
        }

        var popupOthers = Loc.GetString("werewolf-devour-start", ("user", Identity.Entity(uid, EntityManager)), ("target", Identity.Entity(target, EntityManager)));
        _popup.PopupEntity(popupOthers, uid, PopupType.LargeCaution);
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(4), new WerewolfDevourDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }

    public ProtoId<DamageGroupPrototype> DevourDamage = "Brute";
    private void DoDevour(EntityUid uid, WerewolfBasicAbilitiesComponent comp, WerewolfDevourDoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (args.Cancelled
            || HasComp<WerewolfBitComponent>(target)
            || !TryComp<BodyComponent>(target, out var body))
            return;

        var dmg = new DamageSpecifier(_proto.Index(DevourDamage), 35);
        _damage.TryChangeDamage(target, dmg, true, true, targetPart: TargetBodyPart.All);
        RipLimb(target, body);

        EnsureComp<WerewolfBitComponent>(target);

        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return;
        if (!TryComp<WerewolfMindComponent>(mindId, out var mindComp))
            return;
        mindComp.Currency += comp.AmountGut;
        mindComp.BittenPeople.Add(args.Args.Target.Value);

        _hunger.ModifyHunger(uid, +80); // todo maybe put as a var inside a comp or sdome shit
        _audio.PlayPvs(comp.RipSound, uid);
    }

    private void TryGut(EntityUid uid, WerewolfBasicAbilitiesComponent comp, EventWerewolfGut args)
    {
        var target = args.Target;
        if (!HasComp<AbsorbableComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("changeling-absorb-fail-unabsorbable"), uid, uid);
            return;
        }
        _mind.TryGetMind(target, out var mindId, out var mind);
        if (mind == null)
        {
            _popup.PopupEntity(Loc.GetString("werewolf-gut-fail-mind"), uid, uid); // todo locale
            return;
        }
        var popupOthers = Loc.GetString("werewolf-gut-start", ("user", Identity.Entity(uid, EntityManager)), ("target", Identity.Entity(target, EntityManager))); // todo locale
        _popup.PopupEntity(popupOthers, uid, PopupType.LargeCaution);
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(4), new WerewolfGutDoAfterEvent(), uid, target)
        {
            DistanceThreshold = 1.5f,
            BreakOnDamage = true,
            BreakOnHandChange = false,
            BreakOnMove = true,
            BreakOnWeightlessMove = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd,
            MultiplyDelay = false,
        };
        _doAfter.TryStartDoAfter(dargs);
    }

    #endregion
    #region helpers
    private void DoGut(EntityUid uid, WerewolfBasicAbilitiesComponent comp, WerewolfGutDoAfterEvent args)
    {
        if (args.Args.Target == null)
            return;

        var target = args.Args.Target.Value;

        if (args.Cancelled
            || !TryComp<BodyComponent>(target, out var body))
            return;

        if (!TryRemoveOrgan(uid, target, out var removedOrgan))
            return;

        _blood.SpillAllSolutions(target);
        if (_mind.TryGetMind(uid, out var mindId, out _) && TryComp<WerewolfMindComponent>(mindId, out var mindComp))
            mindComp.Currency += comp.AmountGut;

        _hunger.ModifyHunger(uid, +20);
        _audio.PlayPvs(comp.RipSound, uid);
    }


    private bool TryRemoveOrgan(EntityUid user, EntityUid target, out EntityUid? removedOrgan)// chudass taken from devil
    {
        removedOrgan = null;
        var eligibleOrgans = _body.GetBodyOrgans(target).ToList()
            .Where(o => !HasComp<BrainComponent>(o.Id))
            .ToList();

        if (eligibleOrgans.Count <= 0)
        {
            _popup.PopupEntity(Loc.GetString("werewolf-gut-no-organs-left"),  user, user);
            return false;
        }

        var pick = _gambling.Pick(eligibleOrgans);

        _body.RemoveOrgan(pick.Id, pick.Component);
        QueueDel(pick.Id);

        _popup.PopupEntity(
            Loc.GetString("werewolf-gut-success", ("organ", MetaData(pick.Id).EntityName)), // todo locale
            user,
            user);

        return true;
    }
    private void RipLimb(EntityUid target, BodyComponent body)
    {
        var hands = _body.GetBodyChildrenOfType(target, BodyPartType.Arm, body).ToList();

        if (hands.Count <= 0)
            return;

        var pick = _gambling.Pick(hands);

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _wound.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);
    }
    # endregion
}
