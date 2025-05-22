// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.Bloodtrak;
using Content.Server.Forensics;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics.Components;
using Content.Shared.Interaction;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Timing;
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
    [Dependency] private readonly UseDelaySystem _delaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodtrakComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<BloodtrakComponent, ActivateInWorldEvent>(OnActivate);
    }

    /// <summary>
    /// Checks the DNA of the puddle against known DNA entries to find a matching entity.
    /// </summary>
    private EntityUid? GetPuddleDnaOwner(EntityUid target, EntityUid user)
    {
        if (!_tag.HasTag(target, "DNASolutionScannable") || !HasComp<PuddleComponent>(target))
        {
            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-scan-failed"), user, user);
            return null;
        }

        var solutionsDna = _forensicsSystem.GetSolutionsDNA(target);

        if (solutionsDna.Count == 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-no-dna"), user, user);
            return null;
        }

        var targetDna = GetEntityDNAs();

        foreach (var dna in solutionsDna)
        {
            if (!targetDna.TryGetValue(dna, out var uid))
                continue;

            _popupSystem.PopupEntity(Loc.GetString("bloodtrak-dna-saved"), user, user);
            return uid;
        }

        _popupSystem.PopupEntity(Loc.GetString("bloodtrak-no-match"), user, user);
        return null;
    }

    private Dictionary<string, EntityUid> GetEntityDNAs()
    {
        var result = new Dictionary<string, EntityUid>();

        var query = EntityQueryEnumerator<DnaComponent>();
        while (query.MoveNext(out var uid, out var dna))
        {
            if (dna.DNA is not null)
                result[dna.DNA] = uid;
        }

        return result;
    }

    private void OnAfterInteract(EntityUid uid, BloodtrakComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target || component.IsActive || _delaySystem.IsDelayed(uid))
            return;

        args.Handled = true;
        component.Target = GetPuddleDnaOwner(target, args.User);
    }

    public override bool TogglePinpointer(EntityUid uid, BloodtrakComponent? pinpointer = null)
    {
        if (!Resolve(uid, ref pinpointer))
            return false;

        var isActive = !pinpointer.IsActive;

        if (isActive)
        {
            // If the targrt does not exist anymore (deleted, etc), display no target.
            if (pinpointer.Target == null || !Exists(pinpointer.Target.Value))
            {
                _popupSystem.PopupEntity(Loc.GetString("bloodtrak-no-target"), uid);
                return false;
            }

            if (_delaySystem.IsDelayed(uid))
                return false;

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
        if (args.Handled || !args.Complex || _delaySystem.IsDelayed(uid))
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
                _popupSystem.PopupEntity(Loc.GetString(targetValid ? "bloodtrak-tracking-expired" : "bloodtrak-target-lost"), uid);
                TogglePinpointer(uid, tracker);
                tracker.Target = null;

                _delaySystem.SetLength(uid, tracker.CooldownDuration);

                Dirty(uid, tracker);
            }
            else
                UpdateDirectionToTarget(uid, tracker);
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

    private static Shared.Bloodtrak.Distance CalculateDistance(Vector2 vec, BloodtrakComponent pinpointer)
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
