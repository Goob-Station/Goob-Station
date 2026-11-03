using Content.Server.Disposal.Unit;
using Content.Server.Holiday.Christmas;
using Content.Server.Power.EntitySystems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.Doors.Components;
using Content.Shared.EntityConditions;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Mobs.Systems;
using Content.Shared.Prying.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Strip;
using Content.Shared.Strip.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Tools.Components;
using Content.Shared.Trigger.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Achievements.Conditions;

public sealed partial class HugCondition : AchievementCondition
{
    [DataField]
    public bool CreditTarget;

    [DataField]
    public bool SameName;

    [DataField]
    public EntityCondition[]? HuggerRequirements;
}

public sealed partial class AirlockPryCondition : AchievementCondition
{
    [DataField]
    public bool RequireUnpowered = true;
}

public sealed partial class HarmBatonCondition : AchievementCondition;

public sealed partial class UnprotectedWeldingCondition : AchievementCondition;

public sealed partial class PickpocketCondition : AchievementCondition;

public sealed partial class BrainInMmiCondition : AchievementCondition;

public sealed partial class DisposalFlushedCondition : AchievementCondition;

public sealed partial class ReturnToSenderCondition : AchievementCondition;

public sealed partial class RazoriumPatientCondition : AchievementCondition;

public sealed partial class DestroyCondition : AchievementCondition
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();
}

public sealed partial class HugConditionSystem : AchievementConditionSystem<HugCondition>
{
    [Dependency] private SharedEntityConditionsSystem _conditions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, InteractionSuccessEvent>(OnHugged);
    }

    private void OnHugged(Entity<HumanoidAppearanceComponent> ent, ref InteractionSuccessEvent args)
    {
        var hugger = args.User;
        if (hugger == ent.Owner)
            return;

        foreach (var (proto, condition) in Entries)
        {
            if (condition.SameName && Name(hugger) != Name(ent)
                || condition.HuggerRequirements is { } requirements && !_conditions.TryConditions(hugger, requirements))
                continue;

            Achievements.AddProgress(condition.CreditTarget ? ent : hugger, proto);
        }
    }
}

public sealed partial class AirlockPryConditionSystem : AchievementConditionSystem<AirlockPryCondition>
{
    [Dependency] private PowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AirlockComponent, PriedEvent>(OnPried);
    }

    private void OnPried(Entity<AirlockComponent> ent, ref PriedEvent args)
    {
        var powered = _power.IsPowered(ent.Owner);
        foreach (var (proto, condition) in Entries)
        {
            if (condition.RequireUnpowered && powered)
                continue;

            Achievements.AddProgress(args.User, proto);
        }
    }
}

public sealed partial class HarmBatonConditionSystem : AchievementConditionSystem<HarmBatonCondition>
{
    [Dependency] private ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, AttackedEvent>(OnAttacked);
    }

    private void OnAttacked(Entity<HumanoidAppearanceComponent> ent, ref AttackedEvent args)
    {
        if (args.User == ent.Owner || !HasComp<StunbatonComponent>(args.Used) || _toggle.IsActivated(args.Used))
            return;

        ProgressAll(args.User);
    }
}

public sealed partial class UnprotectedWeldingConditionSystem : AchievementConditionSystem<UnprotectedWeldingCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemComponent, ToolUseAttemptEvent>(OnToolUse, before: [typeof(EyeProtectionSystem)]);
    }

    private void OnToolUse(Entity<ItemComponent> ent, ref ToolUseAttemptEvent args)
    {
        if (args.Cancelled
            || !TryComp<RequiresEyeProtectionComponent>(ent, out var requires)
            || !requires.Toggled
            || !TryComp<BlindableComponent>(args.User, out var blindable)
            || blindable.IsBlind)
            return;

        var ev = new GetEyeProtectionEvent();
        RaiseLocalEvent(args.User, ev);

        if (requires.StatusEffectTime > ev.Protection)
            ProgressAll(args.User);
    }
}

public sealed partial class PickpocketConditionSystem : AchievementConditionSystem<PickpocketCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, StrippableDoAfterEvent>(OnStripped, after: [typeof(SharedStrippableSystem)]);
    }

    private void OnStripped(Entity<ActorComponent> ent, ref StrippableDoAfterEvent args)
    {
        if (args.Cancelled || args.InsertOrRemove || args.InventoryOrHand)
            return;

        ProgressAll(ent);
    }
}

public sealed partial class BrainInMmiConditionSystem : AchievementConditionSystem<BrainInMmiCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BrainComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
    }

    private void OnInserted(Entity<BrainComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!HasComp<MMIComponent>(args.Container.Owner))
            return;

        if (Achievements.TryGetSession(ent, out _))
            ProgressAll(ent);
        else
            ProgressAll(args.Container.Owner);
    }
}

public sealed partial class DisposalFlushedConditionSystem : AchievementConditionSystem<DisposalFlushedCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActorComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
    }

    private void OnInserted(Entity<ActorComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (HasComp<DisposalHolderComponent>(args.Container.Owner))
            ProgressAll(ent);
    }
}

public sealed partial class ReturnToSenderConditionSystem : AchievementConditionSystem<ReturnToSenderCondition>
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimerTriggerComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnThrowHit(Entity<TimerTriggerComponent> ent, ref ThrowDoHitEvent args)
    {
        if (!HasComp<ActiveTimerTriggerComponent>(ent)
            || ent.Comp.User is not { } armer
            || args.Target != armer
            || args.Component.Thrower is not { } thrower
            || thrower == armer)
            return;

        ProgressAll(thrower);
    }
}

public sealed partial class RazoriumPatientConditionSystem : AchievementConditionSystem<RazoriumPatientCondition>
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    private const string Razorium = "Razorium";
    private static readonly TimeSpan CheckDelay = TimeSpan.FromSeconds(1);

    private readonly List<(EntityUid Patient, EntityUid Medic, float Before, TimeSpan CheckAt)> _pending = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodstreamComponent, TargetBeforeInjectEvent>(OnInject);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private float GetRazorium(Entity<BloodstreamComponent> patient)
    {
        return _solution.TryGetSolution(patient.Owner, patient.Comp.BloodSolutionName, out _, out var solution)
            ? solution.GetTotalPrototypeQuantity(Razorium).Float()
            : 0f;
    }

    private void OnInject(Entity<BloodstreamComponent> ent, ref TargetBeforeInjectEvent args)
    {
        if (Entries.Count == 0 || args.Cancelled || !_mobState.IsCritical(ent))
            return;

        _pending.Add((ent, args.EntityUsingInjector, GetRazorium(ent), _timing.CurTime + CheckDelay));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            var (patient, medic, before, checkAt) = _pending[i];
            if (checkAt > _timing.CurTime)
                continue;

            _pending.RemoveAt(i);

            if (TryComp<BloodstreamComponent>(patient, out var bloodstream)
                && GetRazorium((patient, bloodstream)) > before)
                ProgressAll(medic);
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _pending.Clear();
    }
}

public sealed partial class DestroyConditionSystem : AchievementConditionSystem<DestroyCondition>
{
    private readonly Dictionary<EntityUid, EntityUid> _lastAttacker = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LimitedItemGiverComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<LimitedItemGiverComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnDamaged(Entity<LimitedItemGiverComponent> ent, ref DamageChangedEvent args)
    {
        if (args.Origin is { } origin)
            _lastAttacker[ent] = Achievements.ResolveAttacker(origin);
    }

    private void OnDestroyed(Entity<LimitedItemGiverComponent> ent, ref DestructionEventArgs args)
    {
        if (!_lastAttacker.Remove(ent, out var attacker) || Prototype(ent) is not { } entProto)
            return;

        foreach (var (proto, condition) in Entries)
        {
            if (condition.Prototypes.Contains(entProto.ID))
                Achievements.AddProgress(attacker, proto);
        }
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _lastAttacker.Clear();
    }
}
