using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Content.Server.Afk;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Events;
using Content.Shared._Goobstation.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.Containers;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Goobstation.PlayerListener;

/// <summary>
///     Notifies if 2 or more players are near a marker for an extended amount of time
///     To trigger, all conditions must be true:
///     0. Mobs in proximity must be humanoid
///     1. X(>1) amount of humanoids are in a Y distance of tiles away from a marker
///     2. At least two humanoids are currently players
///     3. None of the players are dead, crit, AFK or disconnected
/// </summary>
/// <remarks>
///     Fires faster if at least 1 out of 2 or more players has nothing in their body clothing slot.
///     This is called "expedited" here.
/// </remarks>
public sealed class DormNotifier : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IAfkManager _afk = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    private HashSet<CancellationTokenSource> _tokens = [];

    private bool _enabled;
    private int _frequency = 10;

    private int _timeout = 120;
    private int _timeoutExpedited = 25;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, GoobCVars.DormNotifier, value => _enabled = value, true);
        Subs.CVar(_cfg, GoobCVars.DormNotifierFrequency, value => _frequency = value, true);
        Subs.CVar(_cfg, GoobCVars.DormNotifierPresenceTimeout, value => _timeout = value, true);
        Subs.CVar(_cfg, GoobCVars.DormNotifierPresenceTimeoutNude, value => _timeoutExpedited = value, true);

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRestartCleanup);
    }

    private int _clock;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled || _frequency <= 0)
            return;

        if (_clock <= _frequency)
        {
            _clock++;
            return;
        }

        _clock = 0;
        Check();
    }

    private void Check()
    {
        var query = EntityQueryEnumerator<DormNotifierMarkerComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            CheckMarker(uid, comp);
        }
    }

    private void CheckMarker(EntityUid uid, DormNotifierMarkerComponent comp)
    {
        var ents = _lookup.GetEntitiesInRange<HumanoidAppearanceComponent>(Transform(uid).Coordinates, comp.ProximityRadius);
        var found = Validate(uid, ents, out var condemned);

        if (found)
        {
            Condemn(uid, condemned);
        }
    }

    private bool Validate(EntityUid marker, HashSet<Entity<HumanoidAppearanceComponent>> entities, [NotNullWhen(true)] out HashSet<EntityUid> condemned)
    {
        // "0. Mobs in proximity must be humanoid" is handled by Entity<HumanoidAppearanceComponent>
        condemned = [];

        // 1. X(>1) amount of humanoids are in a Y distance of tiles away from a marker
        if (entities.Count < 2)
            return false;

        foreach (var ent in entities)
        {
            // 2. At least two humanoids are currently players
            if (!TryComp<ActorComponent>(ent.Owner, out var actorComp))
                continue;

            // 3. None of the players are dead, crit, AFK or disconnected
            if (TryComp<MobStateComponent>(ent.Owner, out var statecomp) &&
                statecomp.CurrentState is MobState.Critical or MobState.Dead)
                return false;

            if (_afk.IsAfk(actorComp.PlayerSession))
                return false;

            // Disconnected is checked by having ActorComponent present, an entity loses it if the owner disconnects or changes entity.
            condemned.Add(ent.Owner);
        }

        // X(>1) amount of humanoids are in a Y distance of tiles away from a marker
        return condemned.Count > 1;
    }

    private void Condemn(EntityUid marker, HashSet<EntityUid> condemned)
    {
        var potential = new Condemnation(marker, condemned);

        if (AlreadyCondemned(potential))
            return;

        if (!TryGetDormNotifierEntity(out var dnc))
            return;

        var expedited = DetermineExpedited(condemned);

        dnc.Value.Comp.Potential.Add(potential);
        QueueRecheck(TimeSpan.FromSeconds(expedited ? _timeoutExpedited : _timeout), potential, expedited);
    }

    /// <summary>
    /// Determine if this should use an expedited timer based on whether or not any of the mobs have nothing in their suit slot
    /// </summary>
    /// <returns></returns>
    private bool DetermineExpedited(HashSet<EntityUid> sinners)
    {
        foreach (var sinner in sinners)
        {
            // No jumpsuit slot we dont care
            //if (!_slots.TryGetSlot(sinner, "jumpsuit", out var slot))
            if (!_container.TryGetContainer(sinner, "jumpsuit", out var cont))
                continue;

            // Sinner!
            if (cont.Count == 0)
                return true;
        }

        return false;
    }

    private void Recheck(Condemnation condemned, bool expedited = false)
    {
        try
        {
            var sinners = condemned.Condemned
                .Select(con => new Entity<HumanoidAppearanceComponent>(con, Comp<HumanoidAppearanceComponent>(con)))
                .ToHashSet();

            bool valid = Validate(condemned.Marker, sinners, out _);

            if (!valid)
            {
                RemovePotentialCondemned(condemned);
                return;
            }
        }
        catch (KeyNotFoundException e)
        {
            Log.Warning("Entity didn't have HumanoidAppearanceComponent");
        }

        var current = DetermineExpedited(condemned.Condemned);

        // It was expedited
        if (expedited)
        {
            // But isn't now.
            if (!current)
            {
                if (_timeout > _timeoutExpedited)
                {
                    QueueRecheck(TimeSpan.FromSeconds(_timeout - _timeoutExpedited), condemned);
                    return;
                }

                Log.Warning("DormNotifierPresenceTimeoutNude is larger than DormNotifierPresenceTimeout!");
                return;
            }
        }

        // If it wasn't expedited, but is now - oh well.
        Notify(condemned, current);
    }

    private void ApplyPermanently(Condemnation condemn)
    {
        RemovePotentialCondemned(condemn);
        if (TryGetDormNotifierEntity(out var ent))
            ent.Value.Comp.Condemned.Add(condemn);
    }

    private void RemovePotentialCondemned(Condemnation condemn)
    {
        if (TryGetDormNotifierEntity(out var ent))
            ent.Value.Comp.Potential.Remove(condemn);
    }

    private void Notify(Condemnation condemn, bool expedited)
    {
        ApplyPermanently(condemn);
        var names = condemn.Condemned.Select(uid => MetaData(uid).EntityName).ToList();
        var sinners = string.Join(", ", names);

        var message = expedited
            ? Loc.GetString("dorm-condemned-expedited",
                ("dorm", Comp<DormNotifierMarkerComponent>(condemn.Marker).Name),
                ("sinners", sinners))
            : Loc.GetString("dorm-condemned",
                ("dorm", Comp<DormNotifierMarkerComponent>(condemn.Marker).Name));

        _chat.DispatchGlobalAnnouncement(message, colorOverride: Color.Red);
    }

    private bool TryGetDormNotifierEntity([NotNullWhen(true)] out Entity<DormNotifierComponent>? entity)
    {
        entity = null;

        var query = EntityQueryEnumerator<DormNotifierComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            entity = new Entity<DormNotifierComponent>(uid, comp);
        }

        return entity is not null;
    }

    private bool AlreadyCondemned(Condemnation condemnation)
    {
        if (!TryGetDormNotifierEntity(out var ent))
            return false;

        var master = ent.Value.Comp.Condemned.Concat(ent.Value.Comp.Potential);
        var entities = new HashSet<int>(master.SelectMany(m => m.Condemned.Select(c => c.Id)));

        return condemnation.Condemned.Any(con => entities.Contains(con.Id));
    }

    private void QueueRecheck(TimeSpan seconds, Condemnation potential, bool expedited = false)
    {
        var cts = new CancellationTokenSource();
        _tokens.Add(cts);
        Timer.Spawn(seconds, () => Recheck(potential, expedited), cts.Token);
    }

    private void OnRoundStart(RoundStartingEvent args)
    {
        var ent = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<DormNotifierComponent>(ent);
    }

    /// <summary>
    /// > Trigger timer to recheck
    /// > Round ends
    /// > Entity doesnt exist no mo
    /// > Server dies
    /// </summary>
    private void OnRestartCleanup(RoundRestartCleanupEvent args)
    {
        foreach (var token in _tokens)
        {
            token.Cancel();
        }
    }
}
