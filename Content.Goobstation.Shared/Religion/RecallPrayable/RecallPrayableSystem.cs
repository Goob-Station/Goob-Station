using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Religion.RecallPrayable;

public sealed partial class RecallPrayableSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

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
            Text = Loc.GetString(ent.Comp.Verb),
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

        if (!TryComp<BibleUserComponent>(args.User, out var comp) || comp.NullRod == null)
            return;

        if (!TryComp<NullrodComponent>(comp.NullRod, out var nullrodComp))
            return;

        args.Handled = true;

        var nullrod = (comp.NullRod.Value, nullrodComp);

        if (TerminatingOrDeleted(nullrod.Value))
        {
            _popup.PopupClient(Loc.GetString("chaplain-recall-nullrod-gone", ("nullrod", nullrod.Value)), args.User, args.User);
            return;
        }

        RecallNullrod(nullrod, args.User);
    }

    private void RecallNullrod(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        switch (nullrod.Comp.SpecialState)
        {
            case NullrodSpecialState.Normal:
                RecallNormal(nullrod, user);
                break;

            case NullrodSpecialState.Unremoveable:
                RecallUnremoveable(nullrod, user);
                break;

            case NullrodSpecialState.DualWield:
                break;

            case NullrodSpecialState.Embedded:
                break;

            default:
                RecallNormal(nullrod, user);
                break;
        }
    }

    private void RecallNormal(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        var message = _hands.TryPickupAnyHand(user, nullrod)
            ? "chaplain-recall-nullrod-recalled"
            : "chaplain-recall-hands-full";

        _popup.PopupClient(Loc.GetString(message, ("nullrod", nullrod)), user, user);
    }

    private void RecallUnremoveable(Entity<NullrodComponent> nullrod, EntityUid user)
    {
        if (!HasComp<UnremoveableComponent>(nullrod))
            return;

        RemComp<UnremoveableComponent>(nullrod);

        var message = _hands.TryPickupAnyHand(user, nullrod)
            ? "chaplain-recall-nullrod-recalled"
            : "chaplain-recall-hands-full";

        EnsureComp<UnremoveableComponent>(nullrod);

        _popup.PopupClient(Loc.GetString(message, ("nullrod", nullrod)), user, user);
    }
}
