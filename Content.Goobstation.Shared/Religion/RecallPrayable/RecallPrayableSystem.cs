using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.RecallPrayable;

public sealed partial class RecallPrayableSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

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
                    _popup.PopupPredicted(Loc.GetString("chaplain-recall-no-nullrod"), user.User, user.User);
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
        if (args.Cancelled || args.Handled || TerminatingOrDeleted(args.User))
            return;

        if (!TryComp<BibleUserComponent>(args.User, out var comp) || comp.NullRod == null)
            return;

        args.Handled = true;

        var nullrod = comp.NullRod.Value;

        if (TerminatingOrDeleted(nullrod))
        {
            _popup.PopupClient(Loc.GetString("chaplain-recall-nullrod-gone", ("nullrod", nullrod)), args.User, args.User);
            return;
        }
        var xform = Transform(nullrod);

        if (xform.GridUid == null)
            return;

        var message = _hands.TryPickupAnyHand(args.User, nullrod) ? "chaplain-recall-nullrod-recalled" : "chaplain-recall-hands-full";
        _popup.PopupPredicted(Loc.GetString(message, ("nullrod", nullrod)), args.User, args.User);
    }
}
