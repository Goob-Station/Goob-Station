using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.GrabIntent;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.EntityConditions;
using Content.Shared.EntityEffects;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.MartialArts;

/// <summary>
/// TODO: having multiple martial arts doesnt seem to predict properly on client, etc having cqc and judo has the client
/// only know about cqc moves..???
/// </summary>
public sealed partial class MartialArtsComboSystem : EntitySystem
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private MartialArtsSystem _martialArts = default!;

    /// <summary>
    /// Returns whether the second sequence could become first sequence in future with more attacks (or is equal).
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    private static bool DoComboAttackPotentialMatch(
        List<ComboAttackType> first,
        List<ComboAttackType> second)
    {
        // if second exceeds first then nuh uh
        if (first.Count < second.Count)
            return false;

        for (int i = 0; i < second.Count; i++)
        {
            if (first[i] != second[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get future possible combos with more moves being done (will include completed combos aswell)
    /// </summary>
    /// <param name="givenCombos"></param>
    /// <param name="doneAttacks"></param>
    /// <param name="combos"></param>
    /// <returns></returns>
    private bool TryFindPotentialCombos(
        List<ProtoId<MartialArtsComboPrototype>> givenCombos,
        List<ComboAttackType> doneAttacks,
        out List<MartialArtsComboPrototype> combos)
    {
        combos = [];

        foreach (var comboId in givenCombos)
        {
            var comboProto = _proto.Index(comboId);

            if (DoComboAttackPotentialMatch(comboProto.Attacks, doneAttacks))
                combos.Add(comboProto);
        }

        return combos.Count > 0;
    }

    /// <summary>
    /// Get completed combos
    /// </summary>
    /// <param name="givenCombos"></param>
    /// <param name="doneAttacks"></param>
    /// <param name="combos"></param>
    /// <returns></returns>
    private bool TryFindMatchingCombos(
        List<ProtoId<MartialArtsComboPrototype>> givenCombos,
        List<ComboAttackType> doneAttacks,
        out List<MartialArtsComboPrototype> combos)
    {
        combos = [];

        foreach (var comboId in givenCombos)
        {
            var comboProto = _proto.Index(comboId);
            if (comboProto.Attacks.SequenceEqual(doneAttacks))
                combos.Add(comboProto);
        }

        return combos.Count > 0;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ComboPerformerComponent>();
        while (query.MoveNext(out var ent, out var comp))
        {
            if (comp.CurrentTarget != null && TerminatingOrDeleted(comp.CurrentTarget.Value))
                comp.CurrentTarget = null;

            if (_timing.CurTime < comp.ResetTime
                || comp.LastAttacks.Count == 0)
                continue;

            comp.LastAttacks.Clear();
            Dirty(ent, comp);
        }
    }

    [SubscribeLocalEvent]
    private void OnMovePerformed(Entity<ComboPerformerComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (!_timing.IsFirstTimePredicted) // stupid fucking bullshit or something
            return;

        if (args.Cancelled)
            return;

        // SLOP : this is specifically for cqc neck snap stage to keep it intuitive,
        // dont want to perform Slam instead of Neck Snap.
        // not really sure of a better solution here, kinda lazy
        // TODO: Intent modifiers on basic attacks later, such as special behavior for shove and grab and stuff
        // , which would replace neck snap being an actual combo.
        if (args.Type == ComboAttackType.Grab
            && TryComp(ent, out GrabIntentComponent? grabIntent)
            && grabIntent.GrabStage == Common.Grab.GrabStage.Suffocate
            && _martialArts.HasMartialArt(ent.Owner, _proto.Index<MartialArtPrototype>("CloseQuartersCombat")))
            return;

        // Reset combos if we changed targets
        if (ent.Comp.CurrentTarget != null && ent.Comp.CurrentTarget != args.Target)
        {
            Log.Error("Changed targets!");
            ent.Comp.LastAttacks.Clear();
        }

        // Make sure they attacked with their body
        // Todo let combos be performed with specific weapons
        // implement combo.WeaponWhitelist someehow
        if (args.Weapon != ent.Owner)
        {
            Log.Error("Not body");
            ent.Comp.LastAttacks.Clear();
            Dirty(ent);
            return;
        }

        ent.Comp.CurrentTarget = args.Target;
        ent.Comp.ResetTime = _timing.CurTime + ent.Comp.LastAttacksResetDelay;
        ent.Comp.LastAttacks.Add(args.Type);
        Log.Error($"adding {args.Type}");
        Dirty(ent);

        var ev = new AfterComboAttackPerformedEvent(ent, args.Target, args.Weapon, args.Type);
        RaiseLocalEvent(ent, ev);
    }

    [SubscribeLocalEvent]
    private void AfterMovePerformed(Entity<ComboPerformerComponent> ent, ref AfterComboAttackPerformedEvent args)
    {
        if (args.Cancelled)
            return;

        // you can't really do martial arts on someone while you're pulling something else..
        // (also a hacky solution for grab intent condition to make sure they arent grabbing something else than target)
        if (TryComp(ent, out PullerComponent? pullerComp)
            && !TerminatingOrDeleted(pullerComp.Pulling)
            && pullerComp.Pulling != args.Target)
            return;

        // main provider in SharedMartialArtsSystem, other systems should be able to add some too
        // currently CanPerformComboComponent is only added if you learn a martial art though..
        // TODO: perhaps could just give it to everyone since they still need a combo provider to use it..?
        var ev = new GetAvailableCombosEvent(args.Performer, args.Target, args.Weapon, []);
        RaiseLocalEvent(ent, ref ev);
        Log.Error($"Available Combos: {ev.Combos.Count}");
        foreach (var combo in ev.Combos)
        {
            Log.Error(combo.Id);
        }

        // absolutely no possibly combos from our last attacks, clear!
        // will prevent bloat in ui and match code, only show them when theyre doing a valid combo
        if (!TryFindPotentialCombos(ev.Combos, ent.Comp.LastAttacks, out _))
        {
            Log.Error("No potential!");
            ent.Comp.LastAttacks.RemoveRange(0, ent.Comp.LastAttacks.Count - 1); // dont remove latest
            Dirty(ent);
            return;
        }

        if (!TryFindMatchingCombos(ev.Combos, ent.Comp.LastAttacks, out var possibleCombos))
            return;

        foreach (var combo in possibleCombos)
        {
            // this is SLOP but entity conditions/effects arent advanced enough to work for martial arts without this
            // i would rather have this because it is still far cleaner and easier to work with than what we had before
            EnsureComp<MartialArtsComboPerformedComponent>(args.Performer).PerformedOn = args.Target;
            EnsureComp<MartialArtsComboPerformedOnComponent>(args.Target).Performer = ent;

            var success = _conditions.TryConditions(args.Performer, combo.UserConditions)
                && _conditions.TryConditions(args.Target, combo.TargetConditions);

            if (success)
            {
                var identityPerformer = Identity.Entity(args.Performer, EntityManager);
                var identityReceiver = Identity.Entity(args.Target, EntityManager);

                var sPerformer = Loc.GetString(
                    "martial-arts-action-sender",
                    ("performer", identityPerformer),
                    ("receiver", identityReceiver),
                    ("move", combo.Name)
                );
                var sReceiver = Loc.GetString(
                    "martial-arts-action-receiver",
                    ("performer", identityPerformer),
                    ("receiver", identityReceiver),
                    ("move", combo.Name)
                );

                _popup.PopupEntity(sPerformer, args.Performer, args.Performer);
                _popup.PopupEntity(sReceiver, args.Target, args.Target);

                _effects.ApplyEffects(args.Performer, combo.UserEffects);
                _effects.ApplyEffects(args.Target, combo.TargetEffects);

                // TODO: for proper implementation of failEffects must be moved outside of loop,
                // however currently neck snap is a 1-move combo so this fucks it up by instantly clearing the comb
                ent.Comp.LastAttacks.Clear();
                Dirty(ent);
            }
            else
            {
                // not implemented properly see above
                _effects.ApplyEffects(args.Performer, combo.UserFailEffects);
                _effects.ApplyEffects(args.Target, combo.TargetFailEffects);
            }

            RemComp<MartialArtsComboPerformedComponent>(args.Performer);
            RemComp<MartialArtsComboPerformedOnComponent>(args.Target);

            return; // dealing with multiple combos sounds like a bad idea for now
        }
    }
}