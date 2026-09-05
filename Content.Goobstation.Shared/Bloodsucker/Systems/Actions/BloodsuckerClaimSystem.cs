using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerClaimSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerClaimEvent>(OnClaim);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerClaimDoAfterEvent>(OnClaimDoAfter);
    }

    private void OnClaim(Entity<BloodsuckerComponent> ent, ref BloodsuckerClaimEvent args)
    {
        if (!TryComp(ent, out BloodsuckerClaimComponent? comp))
            return;

        var target = args.Target;

        // Clicking on our already-claimed coffin = unclaim
        if (TryComp(ent.Owner, out BloodsuckerClaimedCoffinComponent? claimed)
            && claimed.Coffin == target)
        {
            Unclaim(ent.Owner, claimed);
            args.Handled = true;
            return;
        }

        // Coffin already claimed by someone else
        if (TryComp(target, out BloodsuckerLairComponent? lair) && lair.Owner != ent.Owner)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-claim-already-taken"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Already have a coffin, will unclaim it after the do-after succeeds
        if (TryComp(ent.Owner, out BloodsuckerClaimedCoffinComponent? existing))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-claim-switching"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
        }

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-claim-start"),
            ent.Owner, ent.Owner, PopupType.LargeCaution);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.ClaimDelay,
            new BloodsuckerClaimDoAfterEvent { Coffin = GetNetEntity(target) },
            ent.Owner,
            target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnClaimDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerClaimDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        var coffin = GetEntity(args.Coffin);
        if (!Exists(coffin))
            return;

        // Check again cause someone else may have claimed it during the do-after
        if (TryComp(coffin, out BloodsuckerLairComponent? lair) && lair.Owner != ent.Owner)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-claim-already-taken"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Unclaim existing coffin first
        if (TryComp(ent.Owner, out BloodsuckerClaimedCoffinComponent? existing))
            Unclaim(ent.Owner, existing);

        // Claim new coffin
        var lairComp = EnsureComp<BloodsuckerLairComponent>(coffin);
        lairComp.Owner = ent.Owner;
        Dirty(coffin, lairComp);

        var claimedComp = EnsureComp<BloodsuckerClaimedCoffinComponent>(ent.Owner);
        claimedComp.Coffin = coffin;
        Dirty(ent.Owner, claimedComp);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-claim-success", ("coffin", coffin)),
            ent.Owner, ent.Owner, PopupType.Large);
    }

    private void Unclaim(EntityUid vampire, BloodsuckerClaimedCoffinComponent claimed)
    {
        if (Exists(claimed.Coffin))
            RemComp<BloodsuckerLairComponent>(claimed.Coffin);

        RemComp<BloodsuckerClaimedCoffinComponent>(vampire);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-claim-unclaimed"),
            vampire, vampire, PopupType.Medium);
    }

    // Returns true if the vampire is within their lair radius.
    public bool IsInLair(EntityUid vampire)
    {
        if (!TryComp(vampire, out BloodsuckerClaimedCoffinComponent? claimed))
            return false;

        if (!Exists(claimed.Coffin))
            return false;

        if (!TryComp(vampire, out TransformComponent? vampXform)
            || !TryComp(claimed.Coffin, out TransformComponent? coffinXform))
            return false;

        if (vampXform.MapID != coffinXform.MapID)
            return false;

        var delta = vampXform.WorldPosition - coffinXform.WorldPosition;
        return delta.LengthSquared() <= claimed.LairRadius * claimed.LairRadius;
    }
}
