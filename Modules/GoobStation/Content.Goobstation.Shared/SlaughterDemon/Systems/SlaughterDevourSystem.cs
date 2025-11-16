using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Goobstation.Shared.SlaughterDemon.Other;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.SlaughterDemon.Systems;

/// <summary>
/// This handles the devouring system for the slaughter demons
/// </summary>
public sealed class SlaughterDevourSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    private EntityQuery<PullerComponent> _pullerQuery;
    private EntityQuery<HumanoidAppearanceComponent> _humanoid;
    private EntityQuery<ActorComponent> _actorQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _pullerQuery = GetEntityQuery<PullerComponent>();
        _humanoid = GetEntityQuery<HumanoidAppearanceComponent>();
        _actorQuery = GetEntityQuery<ActorComponent>();

        SubscribeLocalEvent<SlaughterDevourComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlaughterDevourComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);

        SubscribeLocalEvent<SlaughterDevourComponent, SlaughterDevourDoAfter>(OnDoAfter);

        // Drink-related
        SubscribeLocalEvent<DemonsBloodComponent, SlaughterDevourAttemptEvent>(OnAttemptDemonsBlood);
        SubscribeLocalEvent<DemonsKissComponent, SlaughterDevourAttemptEvent>(OnAttemptDemonsKiss);
    }

    private void OnMapInit(Entity<SlaughterDevourComponent> ent, ref MapInitEvent args) =>
        ent.Comp.Container = _container.EnsureContainer<Container>(ent.Owner, "stomach");

    private void OnBloodCrawlAttempt(Entity<SlaughterDevourComponent> ent, ref BloodCrawlAttemptEvent args) =>
        TryDevour(ent.Owner, ent.Comp, ref args);

    private void OnDoAfter(Entity<SlaughterDevourComponent> ent, ref SlaughterDevourDoAfter args)
    {
        if (args.Target == null
            || args.Cancelled)
            return;

        var ev = new SlaughterDevourEvent(args.Target.Value, ent.Owner);
        RaiseLocalEvent(ent.Owner, ref ev, true);
    }

    /// <summary>
    /// Exclusive to slaughter demons. They devour targets once they enter blood crawl jaunt form.
    /// </summary>
    private void TryDevour(EntityUid uid, SlaughterDevourComponent comp, ref BloodCrawlAttemptEvent args)
    {
        if (!_pullerQuery.TryComp(uid, out var puller)
            || puller.Pulling == null)
            return;

        var pullingEnt = puller.Pulling.Value;

        if (_mobState.IsAlive(pullingEnt))
            return;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            comp.DoAfterDelay,
            new SlaughterDevourDoAfter(),
            uid,
            pullingEnt)
        {
            BreakOnMove = true,
            ColorOverride = Color.Red
        };

        args.Cancelled = true; // cancel the jaunt and devour instead

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    #region Drink-related

    private void OnAttemptDemonsBlood(Entity<DemonsBloodComponent> ent, ref SlaughterDevourAttemptEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString("slaughter-demons-blood-devour"), args.Devourer, args.Devourer, PopupType.SmallCaution);
        args.Cancelled = true;
    }

    private void OnAttemptDemonsKiss(Entity<DemonsKissComponent> ent, ref SlaughterDevourAttemptEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _damageable.TryChangeDamage(args.Devourer, ent.Comp.Damage, ignoreResistances: true);
        _popup.PopupEntity(Loc.GetString("slaughter-demons-kiss-devour"), args.Devourer, args.Devourer, PopupType.MediumCaution);

        if (ent.Comp.Eject)
            args.Cancelled = true;
    }
    #endregion

    public void HealAfterDevouring(EntityUid target, EntityUid devourer, SlaughterDevourComponent component)
    {
        // I dont know how to refactor this into events so im leaving it like this
        var toHeal = component.ToHeal;
        if (HasComp<HumanoidAppearanceComponent>(target) && !HasComp<SiliconComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-humanoid"), devourer);
        }
        else if (HasComp<BorgChassisComponent>(target) || HasComp<SiliconComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-robot"), devourer);
            toHeal = component.ToHealNonCrew;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-other"), devourer);
            toHeal = component.ToHealAnythingElse;
        }

        _damageable.TryChangeDamage(devourer,
            toHeal,
            true,
            false,
            targetPart: TargetBodyPart.All,
            splitDamage: SplitDamageBehavior.SplitEnsureAll);
    }

    /// <summary>
    ///  Increments the objectives of the slaughter demons
    /// </summary>
    public void IncrementObjective(EntityUid uid, EntityUid devoured, SlaughterDemonComponent demon)
    {
        if (!_mind.TryGetMind(uid, out _, out var mind))
            return;

        // Goidaaaaaa
        foreach (var objective in mind.Objectives)
        {
            if (TryComp<SlaughterDevourConditionComponent>(objective, out var devourCondition))
                devourCondition.Devour = demon.Devoured;

            if (TryComp<SlaughterKillEveryoneConditionComponent>(objective, out var killEveryoneCondition)
                && _humanoid.HasComp(devoured)
                && _actorQuery.HasComp(devoured))
            {
                killEveryoneCondition.Devoured++;
            }
        }
    }
}
