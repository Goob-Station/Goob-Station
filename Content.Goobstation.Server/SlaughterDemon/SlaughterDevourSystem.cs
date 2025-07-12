// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SlaughterDemon;
using Content.Goobstation.Shared.SlaughterDemon.Objectives;
using Content.Server.Mind;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.SlaughterDemon;

/// <summary>
/// This handles the devouring system for the slaughter demons
/// </summary>
public sealed class SlaughterDevourSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MindSystem _mind = default!;

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

        SubscribeLocalEvent<SlaughterDevourComponent, BloodCrawlAttemptEvent>(OnBloodCrawlAttempt);
    }

    private void OnBloodCrawlAttempt(Entity<SlaughterDevourComponent> ent, ref BloodCrawlAttemptEvent args)
    {
        TryDevour(ent.Owner);
    }

    /// <summary>
    /// Exclusive to slaughter demons. They devour targets once they enter blood crawl jaunt form.
    /// Laughter demons do not directly devour them, however.
    /// </summary>
    private void TryDevour(EntityUid uid)
    {
        if (!_pullerQuery.TryComp(uid, out var puller))
            return;

        if (puller.Pulling == null)
            return;

        var pullingEnt = puller.Pulling.Value;

        if (_mobState.IsAlive(pullingEnt))
            return;

        var ev = new SlaughterDevourEvent(pullingEnt);
        RaiseLocalEvent(uid, ref ev);
    }

    public void HealAfterDevouring(EntityUid target, EntityUid devourer, SlaughterDevourComponent component)
    {
        if (HasComp<HumanoidAppearanceComponent>(target) && !HasComp<SiliconComponent>(target))
        {
            _damageable.TryChangeDamage(devourer, component.ToHeal);
        }
        else if (HasComp<BorgChassisComponent>(target) || HasComp<SiliconComponent>(target))
        {
            _damageable.TryChangeDamage(devourer, component.ToHealNonCrew);
        }
        else
        {
            _damageable.TryChangeDamage(devourer, component.ToHealAnythingElse);
        }
    }

    /// <summary>
    ///  Increments the objectives of the slaughter demons
    /// </summary>
    public void IncrementObjective(EntityUid uid, EntityUid devoured, SlaughterDemonComponent demon)
    {
        // Get the mind entities
        if (_mind.TryGetMind(uid, out _, out var mind))
        {
            // Goidaaaaaa
            foreach (var objective in mind.Objectives)
            {
                if (TryComp<SlaughterDevourConditionComponent>(objective, out var devourCondition))
                {
                    devourCondition.Devour = demon.Devoured;
                }

                if (TryComp<SlaughterKillEveryoneConditionComponent>(objective, out var killEveryoneCondition))
                {
                    if (_humanoid.HasComp(devoured) && _actorQuery.HasComp(devoured))
                    {
                        // We only want to restrict it to actual players
                        killEveryoneCondition.Devoured++;
                    }
                }
            }
        }
    }
}
