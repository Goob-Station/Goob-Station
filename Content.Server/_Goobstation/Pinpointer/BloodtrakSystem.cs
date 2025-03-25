using System.Linq;
using System.Numerics;
using Content.Server.Forensics;
using Content.Server.Shuttles.Events;
using Content.Shared._Gobostation.Pinpointer;
using Content.Shared._Goobstation.Pinpointer;
using Content.Shared.Forensics.Components;
using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Shared.Tag;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Pinpointer;

public sealed class BloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ForensicsSystem _forensicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<BloodtrakComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BloodtrakComponent, ActivateInWorldEvent>(OnActivate);
    }

    private EntityUid GetBloodTarget(EntityUid uid, BloodtrakComponent comp, AfterInteractEvent args)
    {
        // Check if the solution being scanned has DNA associated with it.
        if (args.Target is not { Valid: true } targetEntity || !_tag.HasTag(targetEntity, "DNASolutionScannable"))
        {
            args.Handled = true;
            return default;
        }

        // Get the DNAs of the solution.
        var solutionsDna = _forensicsSystem.GetSolutionsDNA(targetEntity);

        // Early exit if no DNA found
        if (solutionsDna?.Count == 0)
        {
            args.Handled = true;
            return default;
        }

        // Convert to HashSet for O(1) lookups
        var dnaSet = new HashSet<string>(solutionsDna!);

        // Use cached query and avoid closure allocation
        var query = EntityManager.EntityQueryEnumerator<DnaComponent>();
        while (query.MoveNext(out var dnaUid, out var dnaComponent))
        {
            // HashSet.Contains is much faster than iterating through list
            if (dnaSet.Contains(dnaComponent.DNA))
                return dnaUid;
        }

        return default;
    }

    /// <summary>
    ///     Set the target if capable
    /// </summary>
    private void OnAfterInteract(EntityUid uid, BloodtrakComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        if (component.IsActive)
            return;

        args.Handled = true;
        component.Target = GetBloodTarget(uid, component, args);
    }

    /// <summary>
    ///     Set pinpointers target to track
    /// </summary>
    private void SetTarget(EntityUid uid, EntityUid? target, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (pinpointer.Target == target)
            return;

        pinpointer.Target = target;
        if (pinpointer.IsActive)
            UpdateDirectionToTarget(uid, pinpointer);
    }

    public override bool TogglePinpointer(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        var isActive = !pinpointer.IsActive;
        SetActive(uid, isActive, pinpointer);
        UpdateAppearance(uid, pinpointer);
        return isActive;
    }

    private void UpdateAppearance(EntityUid uid, BloodtrakComponent pinpointer, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance))
            return;
        _appearance.SetData(uid, PinpointerVisuals.IsActive, pinpointer.IsActive, appearance);
        _appearance.SetData(uid, PinpointerVisuals.TargetDistance, pinpointer.DistanceToTarget, appearance);
    }

    private void OnActivate(EntityUid uid, BloodtrakComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        TogglePinpointer(uid, component);
        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodtrakComponent>();
        while (query.MoveNext(out var uid, out var tracker))
        {
            // Skip inactive trackers immediately
            if (!tracker.IsActive) continue;

            // Update direction only for active trackers
            UpdateDirectionToTarget(uid, tracker);

            // Check expiration using cached time
            if (_gameTiming.CurTime <= tracker.NextExecutionTime)
                continue;

            // Deactivate and schedule next activation
            tracker.IsActive = false;
            tracker.NextExecutionTime = _gameTiming.CurTime + tracker.TrackingDuration;
            Dirty(uid, tracker);
        }
    }

    /// <summary>
    ///     Try to find the closest entity from whitelist on a current map
    ///     Will return null if can't find anything
    /// </summary>
    private EntityUid? FindTargetFromComponent(EntityUid uid, Type whitelist, TransformComponent? transform = null)
    {
        _xformQuery.Resolve(uid, ref transform, false);

        if (transform == null)
            return null;

        // sort all entities in distance increasing order
        var mapId = transform.MapID;
        var l = new SortedList<float, EntityUid>();
        var worldPos = _transform.GetWorldPosition(transform);

        foreach (var (otherUid, _) in EntityManager.GetAllComponents(whitelist))
        {
            if (!_xformQuery.TryGetComponent(otherUid, out var compXform) || compXform.MapID != mapId)
                continue;

            var dist = (_transform.GetWorldPosition(compXform) - worldPos).LengthSquared();
            l.TryAdd(dist, otherUid);
        }

        // return uid with a smallest distance
        return l.Count > 0 ? l.First().Value : null;
    }

    /// <summary>
    ///     Update direction from pinpointer to selected target (if it was set)
    /// </summary>
    protected override void UpdateDirectionToTarget(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        if (!pinpointer.IsActive)
            return;

        var target = pinpointer.Target;
        if (target == null || !EntityManager.EntityExists(target.Value))
        {
            SetDistance(uid, Shared._Goobstation.Pinpointer.Distance.Unknown, pinpointer);
            return;
        }

        var dirVec = CalculateDirection(uid, target.Value);
        var oldDist = pinpointer.DistanceToTarget;
        if (dirVec != null)
        {
            var angle = dirVec.Value.ToWorldAngle();
            TrySetArrowAngle(uid, angle, pinpointer);
            var dist = CalculateDistance(dirVec.Value, pinpointer);
            SetDistance(uid, dist, pinpointer);
        }
        else
        {
            SetDistance(uid, Shared._Goobstation.Pinpointer.Distance.Unknown, pinpointer);
        }
        if (oldDist != pinpointer.DistanceToTarget)
            UpdateAppearance(uid, pinpointer);
    }

    /// <summary>
    ///     Calculate direction from pinUid to trgUid
    /// </summary>
    /// <returns>Null if failed to calculate distance between two entities</returns>
    private Vector2? CalculateDirection(EntityUid pinUid, EntityUid trgUid)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();

        // check if entities have transform component
        if (!xformQuery.TryGetComponent(pinUid, out var pin))
            return null;
        if (!xformQuery.TryGetComponent(trgUid, out var trg))
            return null;

        // check if they are on same map
        if (pin.MapID != trg.MapID)
            return null;

        // get world direction vector
        var dir = _transform.GetWorldPosition(trg, xformQuery) - _transform.GetWorldPosition(pin, xformQuery);
        return dir;
    }

    private Shared._Goobstation.Pinpointer.Distance CalculateDistance(Vector2 vec, BloodtrakComponent pinpointer)
    {
        var dist = vec.Length();
        if (dist <= pinpointer.ReachedDistance)
            return Shared._Goobstation.Pinpointer.Distance.Reached;
        else if (dist <= pinpointer.CloseDistance)
            return Shared._Goobstation.Pinpointer.Distance.Close;
        else if (dist <= pinpointer.MediumDistance)
            return Shared._Goobstation.Pinpointer.Distance.Medium;
        else
            return Shared._Goobstation.Pinpointer.Distance.Far;
    }
}
