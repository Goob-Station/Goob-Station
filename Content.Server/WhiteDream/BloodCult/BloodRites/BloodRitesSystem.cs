using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.DoAfter;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Content.Shared.WhiteDream.BloodCult.UI;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.BloodRites;

public sealed class BloodRitesSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private readonly ProtoId<ReagentPrototype> _bloodProto = "Blood";

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodRitesAuraComponent, ExaminedEvent>(OnExamining);

        SubscribeLocalEvent<BloodRitesAuraComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BloodRitesAuraComponent, BloodRitesExtractDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<BloodRitesAuraComponent, MeleeHitEvent>(OnCultistHit);

        SubscribeLocalEvent<BloodRitesAuraComponent, BeforeActivatableUIOpenEvent>(BeforeUiOpen);
        SubscribeLocalEvent<BloodRitesAuraComponent, BloodRitesMessage>(OnRitesMessage);
    }

    private void OnExamining(Entity<BloodRitesAuraComponent> rites, ref ExaminedEvent args) =>
        args.PushMarkup(Loc.GetString("blood-rites-stored-blood", ("amount", rites.Comp.StoredBlood.ToString())));

    private void OnAfterInteract(Entity<BloodRitesAuraComponent> rites, ref AfterInteractEvent args)
    {
        if (!args.Target.HasValue || args.Handled || args.Target == args.User ||
            HasComp<BloodCultistComponent>(args.Target))
            return;

        if (HasComp<BloodstreamComponent>(args.Target))
        {
            if (rites.Comp.ExtractDoAfterId.HasValue)
                return;

            var ev = new BloodRitesExtractDoAfterEvent();
            var time = rites.Comp.BloodExtractionTime;
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, time, ev, rites, args.Target)
            {
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                BreakOnDamage = true
            };
            if (_doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
                rites.Comp.ExtractDoAfterId = doAfterId;

            args.Handled = true;
            return;
        }

        if (!TryComp(args.Target, out SolutionContainerManagerComponent? solutionContainer)) // please send help
            return;

        foreach (var (_, solution) in _solutionContainer.EnumerateSolutions((args.Target.Value, solutionContainer)))
        {
            // I don't think something will ever have more than 1000 blood units in it's solution...
            rites.Comp.StoredBlood += solution.Comp.Solution.RemoveReagent(_bloodProto, 1000);
            _solutionContainer.UpdateChemicals(solution);
            break;
        }

        _audio.PlayPvs(rites.Comp.BloodRitesAudio, rites);
        args.Handled = true;
    }

    private void OnDoAfter(Entity<BloodRitesAuraComponent> rites, ref BloodRitesExtractDoAfterEvent args)
    {
        rites.Comp.ExtractDoAfterId = null;
        if (args.Cancelled || args.Handled || args.Target is not { } target ||
            !TryComp(target, out BloodstreamComponent? bloodstream) || bloodstream.BloodSolution is not { } solution)
            return;

        var extracted = solution.Comp.Solution.RemoveReagent(_bloodProto, rites.Comp.BloodExtractionAmount);
        rites.Comp.StoredBlood += extracted;
        _audio.PlayPvs(rites.Comp.BloodRitesAudio, rites);
        args.Handled = true;
    }

    private void OnCultistHit(Entity<BloodRitesAuraComponent> rites, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var playSound = false;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<BloodCultistComponent>(target) && !HasComp<ConstructComponent>(target))
                return;

            if (TryComp(target, out BloodstreamComponent? bloodstream))
            {
                if (RestoreBloodLevel(rites, args.User, (target, bloodstream)))
                    playSound = true;
            }

            if (TryComp(target, out DamageableComponent? damageable))
            {
                if (Heal(rites, args.User, (target, damageable)))
                    playSound = true;
            }
        }

        if (playSound)
            _audio.PlayPvs(rites.Comp.BloodRitesAudio, rites);
    }

    private void BeforeUiOpen(Entity<BloodRitesAuraComponent> rites, ref BeforeActivatableUIOpenEvent args)
    {
        var state = new BloodRitesUiState(rites.Comp.Crafts, rites.Comp.StoredBlood);
        _ui.SetUiState(rites.Owner, BloodRitesUiKey.Key, state);
    }

    private void OnRitesMessage(Entity<BloodRitesAuraComponent> rites, ref BloodRitesMessage args)
    {
        Del(rites);

        var ent = Spawn(args.SelectedProto, _transform.GetMapCoordinates(args.Actor));
        _handsSystem.TryPickup(args.Actor, ent);
    }

    private bool Heal(Entity<BloodRitesAuraComponent> rites, EntityUid user, Entity<DamageableComponent> target)
    {
        if (target.Comp.TotalDamage == 0)
            return false;

        if (TryComp(target, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
        {
            _popup.PopupEntity(Loc.GetString("blood-rites-heal-dead"), target, user);
            return false;
        }

        if (!HasComp<BloodstreamComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("blood-rites-heal-no-bloodstream"), target, user);
            return false;
        }

        var bloodCost = rites.Comp.HealingCost;
        if (target.Owner == user)
            bloodCost *= rites.Comp.SelfHealRatio;

        if (bloodCost >= rites.Comp.StoredBlood)
        {
            _popup.PopupEntity(Loc.GetString("blood-rites-not-enough-blood"), rites, user);
            return false;
        }

        var healingLeft = rites.Comp.TotalHealing;

        foreach (var (type, value) in target.Comp.Damage.DamageDict)
        {
            // somehow?
            if (!_protoManager.TryIndex(type, out DamageTypePrototype? damageType))
                continue;

            var toHeal = value;
            if (toHeal > healingLeft)
                toHeal = healingLeft;

            _damageable.TryChangeDamage(target, new DamageSpecifier(damageType, -toHeal));

            healingLeft -= toHeal;
            if (healingLeft == 0)
                break;
        }

        rites.Comp.StoredBlood -= bloodCost;
        return true;
    }

    private bool RestoreBloodLevel(
        Entity<BloodRitesAuraComponent> rites,
        EntityUid user,
        Entity<BloodstreamComponent> target
    )
    {
        if (target.Comp.BloodSolution is null)
            return false;

        _bloodstream.FlushChemicals(target, "", 10);
        var missingBlood = target.Comp.BloodSolution.Value.Comp.Solution.AvailableVolume;
        if (missingBlood == 0)
            return false;

        var bloodCost = missingBlood * rites.Comp.BloodRegenerationRatio;
        if (target.Owner == user)
            bloodCost *= rites.Comp.SelfHealRatio;

        if (bloodCost > rites.Comp.StoredBlood)
        {
            _popup.PopupEntity("blood-rites-no-blood-left", rites, user);
            bloodCost = rites.Comp.StoredBlood;
        }

        _bloodstream.TryModifyBleedAmount(target, -3);
        _bloodstream.TryModifyBloodLevel(target, bloodCost / rites.Comp.BloodRegenerationRatio);

        rites.Comp.StoredBlood -= bloodCost;
        return true;
    }
}
