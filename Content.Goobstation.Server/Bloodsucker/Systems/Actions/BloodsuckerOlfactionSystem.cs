using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.Bloodsuckers.Systems;
using Content.Server.Forensics;
using Content.Shared.Actions;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics.Components;
using Content.Shared.Localizations;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerOlfactionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerOlfactionEvent>(OnOlfaction);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerOlfactionStopEvent>(OnOlfactionStop);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodsuckerOlfactionComponent>();
        while (query.MoveNext(out var uid, out var olf))
        {
            if (!olf.IsTracking || olf.TrackedTarget == null)
                continue;

            olf.HeartbeatTimer -= frameTime;
            if (olf.HeartbeatTimer > 0f)
                continue;

            olf.HeartbeatTimer = olf.HeartbeatInterval;
            Dirty(uid, olf);

            var target = GetEntity(olf.TrackedTarget.Value);
            if (!Exists(target))
            {
                StopTracking(uid, olf);
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-olfaction-target-lost"),
                    uid, uid, PopupType.MediumCaution);
                continue;
            }

            // Check if target is dead
            if (TryComp(target, out MobStateComponent? mobState)
                && mobState.CurrentState == MobState.Dead)
            {
                StopTracking(uid, olf);
                _popup.PopupEntity(
                    Loc.GetString("bloodsucker-olfaction-target-dead"),
                    uid, uid, PopupType.MediumCaution);
                continue;
            }

            PulseHeartbeat(uid, target, olf);
        }
    }

    private void OnOlfaction(Entity<BloodsuckerComponent> ent, ref BloodsuckerOlfactionEvent args)
    {
        if (!TryComp(ent, out BloodsuckerOlfactionComponent? comp))
            return;

        // If already tracking, stop
        if (comp.IsTracking)
        {
            StopTracking(ent.Owner, comp);
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-olfaction-stopped"),
                ent.Owner, ent.Owner, PopupType.Small);
            args.Handled = true;
            return;
        }

        // Scan for blood puddles nearby
        var target = FindBloodOwner(ent.Owner, comp, args.Target);
        if (target == null)
        {
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-olfaction-no-blood"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (target == ent.Owner)
        {
            _popup.PopupEntity(
                Loc.GetString("bloodsucker-olfaction-own-blood"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        comp.TrackedTarget = GetNetEntity(target.Value);
        comp.IsTracking = true;
        _actions.AddAction(ent.Owner, ref comp.StopActionEntity,
    "ActionBloodsuckerOlfactionStop");
        comp.HeartbeatTimer = 0f; // fire immediately
        Dirty(ent.Owner, comp);

        EnsureComp<BloodsuckerOlfactionOverlayComponent>(ent.Owner);

        _audio.PlayPvs(comp.AcquireSound, ent.Owner);
        _popup.PopupEntity(
            Loc.GetString("bloodsucker-olfaction-acquired", ("target", target.Value)),
            ent.Owner, ent.Owner, PopupType.Medium);

        args.Handled = true;
    }

    private void OnOlfactionStop(Entity<BloodsuckerComponent> ent, ref BloodsuckerOlfactionStopEvent args)
    {
        if (!TryComp(ent, out BloodsuckerOlfactionComponent? comp))
            return;

        StopTracking(ent.Owner, comp);
        args.Handled = true;
    }

    private EntityUid? FindBloodOwner(EntityUid vampire, BloodsuckerOlfactionComponent comp, EntityUid clickedTarget)
    {
        // Check if they clicked directly on a blood puddle
        if (HasComp<PuddleComponent>(clickedTarget))
            return GetDnaOwnerFromPuddle(clickedTarget);

        // Otherwise scan nearby puddles
        foreach (var puddle in _lookup.GetEntitiesInRange<PuddleComponent>(
                     Transform(vampire).Coordinates, comp.ScanRange))
        {
            var owner = GetDnaOwnerFromPuddle(puddle.Owner);
            if (owner != null && owner != vampire)
                return owner;
        }

        return null;
    }

    private EntityUid? GetDnaOwnerFromPuddle(EntityUid puddle)
    {
        // Get DNA from the puddle's solution via forensics
        var solutionsDna = _forensics.GetSolutionsDNA(puddle);
        if (solutionsDna.Count == 0)
            return null;

        // Match DNA to a living entity
        var dnaQuery = EntityQueryEnumerator<DnaComponent>();
        while (dnaQuery.MoveNext(out var uid, out var dna))
        {
            if (dna.DNA == null)
                continue;

            foreach (var (puddleDna, _) in solutionsDna)
            {
                if (puddleDna == dna.DNA)
                    return uid;
            }
        }

        return null;
    }

    private void PulseHeartbeat(EntityUid vampire, EntityUid target, BloodsuckerOlfactionComponent comp)
    {
        var ourCoords = _transform.GetMapCoordinates(vampire);
        var targetCoords = _transform.GetMapCoordinates(target);

        string loc;
        if (targetCoords.MapId != ourCoords.MapId)
        {
            loc = Loc.GetString("bloodsucker-olfaction-faraway");
        }
        else
        {
            var vector = ourCoords.Position - targetCoords.Position;
            var direction = vector.ToWorldAngle().GetDir();
            var locDir = ContentLocalizationManager.FormatDirection(direction).ToLower();
            var dist = vector.Length();

            var distKey = dist switch
            {
                <= 5f => "close",
                <= 15f => "medium",
                _ => "far"
            };

            loc = Loc.GetString("bloodsucker-olfaction-pulse",
                ("direction", locDir),
                ("distance", Loc.GetString($"bloodsucker-olfaction-distance-{distKey}")));
        }

        _popup.PopupEntity(loc, vampire, vampire, PopupType.MediumCaution);
        _audio.PlayPvs(comp.HeartbeatSound, vampire,
            AudioParams.Default.WithVolume(-3f));
    }

    private void StopTracking(EntityUid uid, BloodsuckerOlfactionComponent comp)
    {
        comp.IsTracking = false;
        if (comp.StopActionEntity != null)
        {
            _actions.RemoveAction(uid, comp.StopActionEntity.Value);
            comp.StopActionEntity = null;
        }
        comp.TrackedTarget = null;
        comp.HeartbeatTimer = 0f;
        Dirty(uid, comp);
        RemCompDeferred<BloodsuckerOlfactionOverlayComponent>(uid);
    }
}
