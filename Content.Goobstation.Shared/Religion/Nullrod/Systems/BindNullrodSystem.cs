
using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Religion.Nullrod.Components;
using Content.Shared.Popups;
using Content.Shared.Verbs;

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

        if (!TryComp<BibleUserComponent>(args.User, out var bibleUser))
            return;

        //If null rod already binded to the binded entity then return
        if (bibleUser.NullRod != null && bibleUser.NullRod == ent.Owner)
            return;

        var user = args.User;
        var verb = new ActivationVerb
        {
            Text = Loc.GetString("nullrod-recall-verb-bind"),
            Act = () =>
            {
                bibleUser.NullRod = ent.Owner;
                Dirty(user, bibleUser);

                _popup.PopupClient(Loc.GetString("nullrod-recall-verb-bind-done", ("nullrod", bibleUser.NullRod)), user, user);
            }
        };

        args.Verbs.Add(verb);
    }
}
