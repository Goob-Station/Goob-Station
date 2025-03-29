using System.Numerics;
using Content.Goobstation.Shared.Bloodtrak;
using Content.Server.Forensics;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics.Components;
using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bloodtrak;

public sealed class BloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly ForensicsSystem _forensicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    private readonly Dictionary<string, EntityUid> _dnaMap = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodtrakComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BloodtrakComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<DnaComponent, ComponentStartup>(OnDnaStartup);
        SubscribeLocalEvent<DnaComponent, ComponentRemove>(OnDnaRemoved);
        SubscribeLocalEvent<DnaComponent, EntityTerminatingEvent>(OnDnaTerminating);
    }

    private void OnDnaStartup(EntityUid uid, DnaComponent component, ComponentStartup args)
    {
        _dnaMap[component.DNA] = uid;
    }

    private void OnDnaRemoved(EntityUid uid, DnaComponent component, ComponentRemove args)
    {
        _dnaMap.Remove(component.DNA);
    }

    private void OnDnaTerminating(EntityUid uid, DnaComponent component, ref EntityTerminatingEvent args)
    {
        _dnaMap.Remove(component.DNA);
    }

    /// <summary>
    /// Checks the DNA of the puddle against known DNA entries to find a matching entity.
    /// </summary>
    private EntityUid? GetPuddleDnaOwner(AfterInteractEvent args)
    {
        if (args.Target is not { Valid: true } targetEntity ||
            !_tag.HasTag(targetEntity, "DNASolutionScannable") ||
            !HasComp<PuddleComponent>(targetEntity))
        {
            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-scan-failed"), args.User, args.User);
            args.Handled = true;
            return null;
        }

        var solutionsDna = _forensicsSystem.GetSolutionsDNA(targetEntity);
        if (solutionsDna.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-no-dna"), args.User, args.User);
            args.Handled = true;
            return null;
        }

        foreach (var dna in solutionsDna)
        {
            if (!_dnaMap.TryGetValue(dna, out var uid))
                continue;

            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-dna-saved"), args.User, args.User);
            return uid;
        }

        _popupSystem.PopupPredicted(Loc.GetString("bloodtrak-no-match"), args.User, args.User);
        args.Handled = true;
        return null;
    }

    private void OnAfterInteract(EntityUid uid, BloodtrakComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target || component.IsActive)
            return;

        args.Handled = true;
        component.Target = GetPuddleDnaOwner(args);
    }

    public override bool TogglePinpointer(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        var isActive = !pinpointer.IsActive;

        if (isActive)
        {
            // Allow activation only if BOTH conditions are met:
            // 1. Not in cooldown
            // 2. Has valid target
            if (_gameTiming.CurTime < pinpointer.CooldownEndTime)
            {
                // Math that i don't feel like explaining!
                var remaining = pinpointer.CooldownEndTime - _gameTiming.CurTime;
                var secondsLeft = (float)Math.Max(0, remaining.TotalSeconds); // Ensure non-negative

                var displaySeconds = Math.Ceiling(secondsLeft);
                var popUp = Loc.GetString("bloodtrak-cooldown-active", ("num", displaySeconds));

                _popupSystem.PopupPredicted(popUp,
                    pinpointer.Owner,
                    pinpointer.Owner);
                return false;
            }

            // If the targrt does not exist anymore (deleted, etc), display no target.
            if (pinpointer.Target == null || !Exists(pinpointer.Target.Value))
            {
                _popupSystem.PopupPredicted(Loc.GetString("bloodtrak-no-target"),
                    pinpointer.Owner,
                    pinpointer.Owner);
                return false;
            }

            pinpointer.ExpirationTime = _gameTiming.CurTime + pinpointer.TrackingDuration;
        }

        SetActive(uid, isActive, pinpointer);
        UpdateAppearance(uid, pinpointer);
        return isActive;
    }

    private void UpdateAppearance(EntityUid uid, BloodtrakComponent? pinpointer, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref appearance) || !Resolve(uid, ref pinpointer))
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
        var currentTime = _gameTiming.CurTime;

        var query = EntityQueryEnumerator<BloodtrakComponent>();
        while (query.MoveNext(out var uid, out var tracker))
        {
            if (!tracker.IsActive)
                continue;

            // Check if tracking expired or target is invalid
            var targetValid = tracker.Target != null && Exists(tracker.Target.Value);
            var expired = currentTime >= tracker.ExpirationTime;

            if (!targetValid || expired)
            {
                // Deactivate only if target is invalid or time expired
                _popupSystem.PopupPredicted(
                    Loc.GetString(targetValid ? "bloodtrak-tracking-expired" : "bloodtrak-target-lost"),
                    tracker.Owner,
                    tracker.Owner
                );
                TogglePinpointer(uid, tracker);
                tracker.Target = null;
                tracker.CooldownEndTime = currentTime + tracker.CooldownDuration;
                Dirty(uid, tracker);
            }
            else
            {
                // Target is valid and tracking is active: update direction
                UpdateDirectionToTarget(uid, tracker);
            }
        }
    }

    protected override void UpdateDirectionToTarget(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return;

        var oldDist = pinpointer.DistanceToTarget;
        var target = pinpointer.Target;

        if (target == null || !Exists(target.Value))
        {
            SetDistance(uid, Shared.Bloodtrak.Distance.Unknown, pinpointer);
            return;
        }

        var dirVec = CalculateDirection(uid, target.Value);
        if (dirVec == null)
        {
            SetDistance(uid, Shared.Bloodtrak.Distance.Unknown, pinpointer);
            return;
        }

        var angle = dirVec.Value.ToWorldAngle();
        TrySetArrowAngle(uid, angle, pinpointer);
        var dist = CalculateDistance(dirVec.Value, pinpointer);
        SetDistance(uid, dist, pinpointer);

        if (oldDist != pinpointer.DistanceToTarget)
            UpdateAppearance(uid, pinpointer);
    }

    private Vector2? CalculateDirection(EntityUid pinUid, EntityUid trgUid)
    {
        var xformQuery = GetEntityQuery<TransformComponent>();

        if (!xformQuery.TryGetComponent(pinUid, out var pin) ||
            !xformQuery.TryGetComponent(trgUid, out var trg) ||
            pin.MapID != trg.MapID)
            return null;

        return _transform.GetWorldPosition(trg, xformQuery) - _transform.GetWorldPosition(pin, xformQuery);
    }

    private Shared.Bloodtrak.Distance CalculateDistance(Vector2 vec, BloodtrakComponent pinpointer)
    {
        var dist = vec.Length();

        // Check from smallest to largest threshold
        if (dist <= pinpointer.ReachedDistance)
            return Shared.Bloodtrak.Distance.Reached;
        if (dist <= pinpointer.CloseDistance)
            return Shared.Bloodtrak.Distance.Close;
        if (dist <= pinpointer.MediumDistance)
            return Shared.Bloodtrak.Distance.Medium;

        return Shared.Bloodtrak.Distance.Far;
    }
}
