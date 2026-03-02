using System.Linq;
using Content.Goobstation.Common.Changeling;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Changeling.Objectives.Components;
using Content.Goobstation.Server.Werewolf.Components;
using Content.Goobstation.Shared.Changeling.Actions;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.Werewolf.Abilities;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Mind;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Store.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Store.Components;
using Content.Shared.Throwing;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Werewolf.Systems.Abilities;


public sealed class WerewolfBasicAbilitiesSystem : EntitySystem
{

    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly BloodstreamSystem _blood = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _gambling = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SharedWerewolfBasicAbilitiesSystem _sharedWerewolf = default!; // hell.
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, TransfurmEvent>(TryTransfurm);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfChangeType>(OnChangeType);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfOpenStore>(OnOpenStore);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfDevour>(TryDevour);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, WerewolfDevourDoAfterEvent>(DoDevour);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, PolymorphedEvent>(OnPolymorphed);
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfRegen>(TryRegen);
    }
    # region basic handlers
    private void TryTransfurm(EntityUid uid, WerewolfBasicAbilitiesComponent component, TransfurmEvent args)
    {
        if (component.Transfurmed)
        {
            component.Transfurmed = false;
            _polymorph.Revert(uid);
            // _sharedWerewolf.SyncActions(uid, component);
            args.Handled = true;
            return;
        }

        component.Transfurmed = true;
        _polymorph.PolymorphEntity(uid, component.CurrentMutation);
        component.Transfurmed = false; // trust this is really important, the fucking polymorph is shit!!!!
        args.Handled = true;
    }

    private void OnPolymorphed(EntityUid uid, WerewolfBasicAbilitiesComponent comp, PolymorphedEvent args)
    {
        if (!comp.Transfurmed)
        {
            RemComp<MartialArtsKnowledgeComponent>(uid); // bruh we love MA shitcod
            RemComp<CanPerformComboComponent>(uid);
            _polymorph.CopyPolymorphComponent<HungerComponent>(uid, args.NewEntity);
            if (TryComp<HungerComponent>(uid, out var oldHunger)) // Transfer hunger value
                _hunger.SetHunger(args.NewEntity, _hunger.GetHunger(oldHunger));
            return;
        }
        _polymorph.CopyPolymorphComponent<WerewolfBasicAbilitiesComponent>(uid, args.NewEntity);
        _polymorph.CopyPolymorphComponent<HungerComponent>(uid, args.NewEntity);
        if (TryComp<HungerComponent>(uid, out var oldHungerTakeTwo)) // Transfer hunger value
            _hunger.SetHunger(args.NewEntity, _hunger.GetHunger(oldHungerTakeTwo));

        // _sharedWerewolf.SyncActions(args.NewEntity, Comp<WerewolfBasicAbilitiesComponent>(args.NewEntity)); // todo
        var werewolf = Comp<WerewolfBasicAbilitiesComponent>(args.NewEntity);
        // werewolf.ActionEntities.Clear();
        _sharedWerewolf.SyncActions(args.NewEntity, werewolf);
    }

    private void OnOpenStore(Entity<WerewolfBasicAbilitiesComponent> ent, ref EventWerewolfOpenStore args)
    {
        if (!TryComp<StoreComponent>(ent, out var store)
            || ent.Comp.Transfurmed == true)
            return;

        _mind.TryGetMind(ent.Owner, out var mindId, out var mind);
        if (!TryComp<WerewolfMindComponent>(mindId, out var mindComp)) return;
        foreach (var bit in mindComp.BittenPeople)
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { "Fury", ent.Comp.Amount } }, ent.Owner);
        mindComp.BittenPeople.Clear();

        _store.ToggleUi(ent, ent, store);
        ent.Comp.StoreOpened = true;
    }

    private void OnChangeType(EntityUid uid, WerewolfBasicAbilitiesComponent comp, EventWerewolfChangeType args)
    {
        comp.CurrentMutation = args.WerewolfType;
        _popup.PopupEntity(Loc.GetString("werewolf-mutation-changed", ("mutation", args.WerewolfType)), uid, uid); // todo locale

        args.Handled = true;
    }

    #endregion

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
        var dargs = new DoAfterArgs(EntityManager, uid, TimeSpan.FromSeconds(2), new WerewolfDevourDoAfterEvent(), uid, target)
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
        _blood.SpillAllSolutions(target);
        RipLimb(target, body);

        EnsureComp<WerewolfBitComponent>(target);

        _mind.TryGetMind(uid, out var mindId, out var mind);
        if (!TryComp<WerewolfMindComponent>(mindId, out var mindComp))
            return;

        mindComp.BittenPeople.Add(args.Args.Target.Value);
        _hunger.ModifyHunger(uid, +80); // todo maybe put as a var inside a comp or sdome shit
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

    # region actions
    private bool TryInjectReagents(EntityUid uid, Dictionary<string, FixedPoint2> reagents)
    {
        var solution = new Solution();
        foreach (var (reagentId, quantity) in reagents)
            solution.AddReagent(reagentId, quantity);

        if (!_solution.TryGetInjectableSolution(uid, out var targetSolution, out _))
            return false;

        return _solution.TryAddSolution(targetSolution.Value, solution);
    }

    public void TryRegen(EntityUid uid, WerewolfBasicAbilitiesComponent comp, EventWerewolfRegen args)
    {
        var reagents = new Dictionary<string, FixedPoint2> // i hate fixedpoint bru
        {
            ["Ichor"] = FixedPoint2.New(10),
            ["TranexamicAcid"] = FixedPoint2.New(5)
        };

        if (TryInjectReagents(uid, reagents))
            _popup.PopupEntity(Loc.GetString("werewolf-action-regen-success"), uid, uid);
        args.Handled = true;
    }
    #endregion
}
