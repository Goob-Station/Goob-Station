using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Goobstation.Shared.Weapons.RequiresDualWield;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Religion.Recall;

public sealed partial class RecallPrayableSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RecallPrayableComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
        SubscribeLocalEvent<RecallPrayableComponent, RecallPrayDoAfterEvent>(OnDoAfter);
    }

    public void OnGetVerb(Entity<RecallPrayableComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !TryComp<BibleUserComponent>(args.User, out var bibleUserComp))
            return;

        var user = (args.User, bibleUserComp);
        var altar = (ent.Owner, ent.Comp);

        var recallVerb = new ActivationVerb
        {
            Text = Loc.GetString("chaplain-recall-verb"),
            Act = () =>
            {
                if (bibleUserComp.NullRod == null)
                {
                    _popup.PopupClient(Loc.GetString("chaplain-recall-no-nullrod"), user.User, user.User);
                    return;
                }
                StartRecallPrayDoAfter(user, altar);
            },
        };

        args.Verbs.Add(recallVerb);
    }

    private void StartRecallPrayDoAfter(Entity<BibleUserComponent> user, Entity<RecallPrayableComponent> altar)
    {
        var doAfterArgs = new DoAfterArgs(EntityManager, user, altar.Comp.DoAfterDuration, new RecallPrayDoAfterEvent(), altar.Owner)
        {
            BreakOnMove = true,
            NeedHand = true
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<RecallPrayableComponent> ent, ref RecallPrayDoAfterEvent args)
    {
        //I don't know why adding this check will not crash your client
        if (_net.IsClient)
            return;

        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        //If there is no empty hand then do nothing
        if (!_hands.TryGetEmptyHand(args.User, out var _))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-hands-full"), args.User, args.User);
            return;
        }

        if (!TryComp<BibleUserComponent>(args.User, out var comp) || comp.NullRod == null)
            return;

        if (!TryComp<NullrodComponent>(comp.NullRod, out var nullrodComp))
            return;

        args.Handled = true;

        var nullrod = (comp.NullRod.Value, nullrodComp);

        if (TerminatingOrDeleted(nullrod.Value))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-gone", ("nullrod", nullrod.Value)), args.User, args.User);
            return;
        }

        //If nullrod already in user hands and it is not a dual wield nullrod then do nothing
        if (_hands.IsHolding(args.User, nullrod.Value) && !HasComp<RequiresDualWieldComponent>(nullrod.Value))
        {
            _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-already-in-hand", ("nullrod", nullrod.Value)), args.User, args.User);
            return;
        }

        RecallNullrod(nullrod, args.User);
    }

    private void RecallNullrod(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        switch (nullrod.Comp.RecallType)
        {
            case NullrodRecallType.Normal:
                RecallNormal(nullrod, user);
                break;

            case NullrodRecallType.Unremoveable:
                RecallUnremoveable(nullrod, user);
                break;

            case NullrodRecallType.DualWield:
                RecallDualWield(nullrod, user);
                break;

            case NullrodRecallType.Embedded:
                RecallEmbedded(nullrod, user);
                break;
        }
    }

    private void RecallNormal(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        if (!_hands.TryPickupAnyHand(user, nullrod.Owner))
            return;

        _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-recalled", ("nullrod", nullrod)), user, user);
    }

    private void RecallUnremoveable(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        if (!HasComp<UnremoveableComponent>(nullrod))
            return;

        RemComp<UnremoveableComponent>(nullrod);

        if (!_hands.TryPickupAnyHand(user, nullrod.Owner))
            return;

        EnsureComp<UnremoveableComponent>(nullrod);

        _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-recalled", ("nullrod", nullrod)), user, user);
    }

    private void RecallDualWield(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        if (!TryComp<RequiresDualWieldComponent>(nullrod, out var dualWieldComp))
            return;

        if (!_hands.IsHolding(user, nullrod.Owner))
            _hands.TryPickupAnyHand(user, nullrod.Owner);

        var query = EntityQueryEnumerator<RequiresDualWieldComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_whitelist.IsWhitelistPass(dualWieldComp.Whitelist, uid))
            {
                _hands.TryPickupAnyHand(user, uid);
                break;
            }
        }

        _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-recalled", ("nullrod", nullrod)), user, user);
    }

    private void RecallEmbedded(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        if (!TryComp<EmbeddableProjectileComponent>(nullrod.Owner, out var embedComp))
            return;

        //If it is not embedded then just recall normally
        if (embedComp.EmbeddedIntoUid == null)
        {
            RecallNormal(nullrod, user);
            return;
        }

        _projectile.EmbedDetach(nullrod.Owner, embedComp);

        if (!_hands.TryPickupAnyHand(user, nullrod.Owner))
            return;

        _popup.PopupEntity(Loc.GetString("chaplain-recall-nullrod-recalled", ("nullrod", nullrod)), user, user);
    }
}
