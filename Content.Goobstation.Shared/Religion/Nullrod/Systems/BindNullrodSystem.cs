
using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Religion.Nullrod.Systems;
public sealed partial class BindNullrodSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NullrodComponent, GetVerbsEvent<ActivationVerb>>(OnGetVerb);
    }
    private void OnGetVerb(Entity<NullrodComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BibleUserComponent>(args.User, out var bibleComp))
            return;

        //If null rod already binded to the binded entity then return
        if (bibleComp.NullRod != null && bibleComp.NullRod == ent.Owner)
            return;

        var user = args.User;
        var verb = new ActivationVerb
        {
            Text = Loc.GetString("nullrod-recall-verb-bind"),
            Act = () =>
            {
                bibleComp.NullRod = ent.Owner;
                Dirty(ent);

                _popup.PopupPredicted(Loc.GetString("nullrod-recall-verb-bind-done", ("nullrod", bibleComp.NullRod)), user, user);
            }
        };

        args.Verbs.Add(verb);
    }
}
