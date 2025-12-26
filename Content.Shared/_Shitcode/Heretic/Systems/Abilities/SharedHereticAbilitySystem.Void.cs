using System.Linq;
using Content.Goobstation.Common.BlockTeleport;
using Content.Goobstation.Common.Body.Components;
using Content.Goobstation.Common.Temperature.Components;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Heretic;
using Content.Shared.Interaction;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem
{
    protected virtual void SubscribeVoid()
    {
        SubscribeLocalEvent<HereticComponent, HereticAristocratWayEvent>(OnAristocratWay);
        SubscribeLocalEvent<HereticComponent, HereticVoidBlinkEvent>(OnVoidBlink);
        SubscribeLocalEvent<HereticComponent, HereticVoidPullEvent>(OnVoidPull);
        SubscribeLocalEvent<HereticComponent, HereticVoidConduitEvent>(OnVoidConduit);
    }

    private void OnAristocratWay(Entity<HereticComponent> ent, ref HereticAristocratWayEvent args)
    {
        EnsureComp<SpecialLowTempImmunityComponent>(ent);
        if (args.GrantBreathingImmunity)
            EnsureComp<SpecialBreathingImmunityComponent>(ent);
    }

    private void OnVoidConduit(Entity<HereticComponent> ent, ref HereticVoidConduitEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        args.Handled = true;

        PredictedSpawnAtPosition(args.VoidConduit, Transform(ent).Coordinates);
    }

    private void OnVoidBlink(Entity<HereticComponent> ent, ref HereticVoidBlinkEvent args)
    {
        var ev = new TeleportAttemptEvent();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
            return;

        if (!TryUseAbility(ent, args))
            return;

        var target = _transform.ToMapCoordinates(args.Target);
        if (!_examine.InRangeUnOccluded(ent, target, SharedInteractionSystem.MaxRaycastRange))
        {
            // can only dash if the destination is visible on screen
            Popup.PopupClient(Loc.GetString("dash-ability-cant-see"), ent, ent);
            return;
        }

        var people = GetNearbyPeople(ent, args.Radius, ent.Comp.CurrentPath);
        var xform = Transform(ent);

        PredictedSpawnAtPosition(args.InEffect, xform.Coordinates);
        _transform.SetCoordinates(ent, xform, args.Target);
        PredictedSpawnAtPosition(args.OutEffect, args.Target);

        var condition = ent.Comp.CurrentPath == "Void";

        people.AddRange(GetNearbyPeople(ent, args.Radius, ent.Comp.CurrentPath));
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

    private void OnVoidPull(Entity<HereticComponent> ent, ref HereticVoidPullEvent args)
    {
        if (!TryUseAbility(ent, args))
            return;

        var path = ent.Comp.CurrentPath;
        var condition = ent.Comp.CurrentPath == "Void";
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

        args.Handled = true;
    }
}
