using System.Diagnostics;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.CombatMode;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MisandryBox.PlaytimeDegrade;

/// <summary>
/// Degrades mob stats and movement speed depending on playtime of that particular job.
/// </summary>
public sealed partial class PlaytimeDegrade : JobSpecial
{
    [Dependency] private readonly ISharedPlaytimeManager _playtime = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly IEntityManager _ent = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    // You can't IoC these y'kno?
    private MobThresholdSystem _mob = default!;
    private MovementSpeedModifierSystem _movement = default!;
    private SharedJobSystem _job = default!;
    private SharedMindSystem _mind = default!;

    /// <summary>
    /// Start decaying stats after playing this amount of minutes
    /// </summary>
    [DataField(required: true)]
    public int Since { get; set; }

    /// <summary>
    /// How much to decay by for every minute of playtime beyond Since
    /// </summary>
    [DataField]
    public float? By { get; set; } = null;

    /// <summary>
    /// Start decaying until this amount of playtime reached. This will be the floor for stats.
    /// </summary>
    [DataField(required: true)]
    public int Until { get; set; }

    /// <summary>
    /// Floor for the decay, in percent. By default 25%
    /// </summary>
    [DataField]
    public float Floor { get; set; } = 0.25f;

    /// <summary>
    /// Apply <see cref="DisarmMalusComponent"/> at halfway point?
    /// </summary>
    [DataField]
    public bool? DisarmMalus { get; set; }

    public override void AfterEquip(EntityUid mob)
    {
        IoCManager.InjectDependencies(this);
        _sysMan.Resolve(ref _mob, ref _movement, ref _job, ref _mind);

        DebugTools.Assert(Until <= Since, "PlaytimeDegrade Until must be greater than Since");
        if (Until <= Since)
            return;

        if (!_player.TryGetSessionByEntity(mob, out var session) ||
            !_mind.TryGetMind(mob, out var mind, out var _) ||
            !_job.MindTryGetJob(mind, out var jobPrototype))
            return;

        var playtimes = _playtime.GetPlayTimes(session);
        var jobPlaytime = playtimes.FirstOrNull(e => e.Key == jobPrototype.ID);

        if (jobPlaytime is null)
            return;

        var totalMinutes = (int)jobPlaytime.Value.Value.TotalMinutes;

        if (totalMinutes < Since)
            return;

        if (!_mob.TryGetThresholdForState(mob, MobState.Critical, out var criticalThreshold) ||
            !_mob.TryGetThresholdForState(mob, MobState.Dead, out var deadThreshold))
            return;

        var effectiveMinutes = Math.Min(totalMinutes, Until) - Since;
        var totalDecayMinutes = Until - Since;
        var decayRatio = (float)effectiveMinutes / totalDecayMinutes;

        var critFloor = criticalThreshold.Value * Floor;
        var deadFloor = deadThreshold.Value * Floor;

        FixedPoint2 newCritThreshold, newDeadThreshold;

        if (By.HasValue)
        {
            var totalDecay = By.Value * effectiveMinutes;
            newCritThreshold = FixedPoint2.Max(critFloor, criticalThreshold.Value - totalDecay);
            newDeadThreshold = FixedPoint2.Max(deadFloor, deadThreshold.Value - totalDecay);
        }
        else
        {
            var critDecay = (criticalThreshold.Value - critFloor) * decayRatio;
            var deadDecay = (deadThreshold.Value - deadFloor) * decayRatio;

            newCritThreshold = criticalThreshold.Value - critDecay;
            newDeadThreshold = deadThreshold.Value - deadDecay;
        }

        _mob.SetMobStateThreshold(mob, newCritThreshold, MobState.Critical);
        _mob.SetMobStateThreshold(mob, newDeadThreshold, MobState.Dead);

        if (DisarmMalus.HasValue && DisarmMalus.Value && decayRatio >= 0.5f)
        {
            _ent.EnsureComponent<DisarmMalusComponent>(mob, out var malus);
            malus.Malus = decayRatio;
        }

        if (_ent.TryGetComponent<MovementSpeedModifierComponent>(mob, out var moveComp))
        {
            var speedRatio = 1.0f - (decayRatio * (1.0f - Floor));
            var newWalkSpeed = moveComp.BaseWalkSpeed * speedRatio;
            var newSprintSpeed = moveComp.BaseSprintSpeed * speedRatio;

            _movement.ChangeBaseSpeed(mob, newWalkSpeed, newSprintSpeed, moveComp.BaseAcceleration, moveComp);
            _movement.RefreshMovementSpeedModifiers(mob);
        }
    }
}
