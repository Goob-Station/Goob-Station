using System.Linq;
using Content.Goobstation.Common.BlockTeleport;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Heretic;
using Content.Shared.Interaction;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeVoid()
    {
        SubscribeLocalEvent<HereticVoidBlinkEvent>(OnVoidBlink);
        SubscribeLocalEvent<HereticVoidPullEvent>(OnVoidPull);
        SubscribeLocalEvent<HereticVoidConduitEvent>(OnVoidConduit);
    }

    private void OnVoidConduit(HereticVoidConduitEvent args)
    {
        if (!TryUseAbility(args))
            return;

        PredictedSpawnAtPosition(args.VoidConduit, Transform(args.Performer).Coordinates);
    }

    private void OnVoidBlink(HereticVoidBlinkEvent args)
    {
        if (!TryUseAbility(args, false))
            return;

        Heretic.TryGetHereticComponent(args.Performer, out var heretic, out _);

        var ent = args.Performer;

        var path = heretic?.CurrentPath ?? "Void";

        var ev = new TeleportAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return;

        var target = _transform.ToMapCoordinates(args.Target);
        if (!_examine.InRangeUnOccluded(ent, target, SharedInteractionSystem.MaxRaycastRange))
        {
            // can only dash if the destination is visible on screen
            Popup.PopupClient(Loc.GetString("dash-ability-cant-see"), ent, ent);
            return;
        }

        var people = GetNearbyPeople(ent, args.Radius, path);
        var xform = Transform(ent);

        PredictedSpawnAtPosition(args.InEffect, xform.Coordinates);
        _transform.SetCoordinates(ent, xform, args.Target);
        PredictedSpawnAtPosition(args.OutEffect, args.Target);

        var condition = path == "Void";

        people.AddRange(GetNearbyPeople(ent, args.Radius, path));
        foreach (var pookie in people.ToHashSet())
        {
            if (condition)
                Voidcurse.DoCurse(pookie, 2);
            _dmg.TryChangeDamage(pookie,
                args.Damage * _body.GetVitalBodyPartRatio(pookie),
                true,
                origin: ent,
                targetPart: TargetBodyPart.All,
                canMiss: false);
        }

        args.Handled = true;
    }

    private void OnVoidPull(HereticVoidPullEvent args)
    {
        if (!TryUseAbility(args))
            return;

        Heretic.TryGetHereticComponent(args.Performer, out var heretic, out _);

        var ent = args.Performer;

        var path = heretic?.CurrentPath ?? "Void";
        var condition = path == "Void";
        var coords = Transform(ent).Coordinates;

        var pookies = GetNearbyPeople(ent, args.Radius, path);
        foreach (var pookie in pookies)
        {
            _dmg.TryChangeDamage(pookie,
                args.Damage * _body.GetVitalBodyPartRatio(pookie),
                true,
                origin: ent,
                targetPart: TargetBodyPart.All,
                canMiss: false);

            _stun.TryStun(pookie, args.StunTime, true);
            _stun.TryKnockdown(pookie, args.KnockDownTime, true);

            if (condition)
                Voidcurse.DoCurse(pookie, 3);

            _throw.TryThrow(pookie, coords);
        }

        PredictedSpawnAtPosition(args.InEffect, coords);
    }
}
