using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Damage;
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
    }

    private void OnMapInit(Entity<SlaughterDevourComponent> ent, ref MapInitEvent args) =>
        ent.Comp.Container = _container.EnsureContainer<Container>(ent.Owner, "stomach");

    private void OnBloodCrawlAttempt(Entity<SlaughterDevourComponent> ent, ref BloodCrawlAttemptEvent args) =>
        TryDevour(ent.Owner);

    /// <summary>
    /// Exclusive to slaughter demons. They devour targets once they enter blood crawl jaunt form.
    /// Laughter demons do not directly devour them, however.
    /// </summary>
    private void TryDevour(EntityUid uid)
    {
        if (!_pullerQuery.TryComp(uid, out var puller)
            || puller.Pulling == null)
            return;

        var pullingEnt = puller.Pulling.Value;

        if (_mobState.IsAlive(pullingEnt))
            return;

        var ev = new SlaughterDevourEvent(pullingEnt, Transform(uid).Coordinates);
        RaiseLocalEvent(uid, ref ev);
    }

    public void HealAfterDevouring(EntityUid target, EntityUid devourer, SlaughterDevourComponent component)
    {
        // I dont know how to refactor this into events so im leaving it like this
        if (HasComp<HumanoidAppearanceComponent>(target) && !HasComp<SiliconComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-humanoid"), devourer);
            _damageable.TryChangeDamage(devourer, component.ToHeal);
        }
        else if (HasComp<BorgChassisComponent>(target) || HasComp<SiliconComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-robot"), devourer);
            _damageable.TryChangeDamage(devourer, component.ToHealNonCrew);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("slaughter-devour-other"), devourer);
            _damageable.TryChangeDamage(devourer, component.ToHealAnythingElse);
        }
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
