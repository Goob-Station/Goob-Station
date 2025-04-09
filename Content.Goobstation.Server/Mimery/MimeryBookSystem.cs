using Content.Server.Abilities.Mime;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Mimery;

public sealed class MimeryBookSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;



    public override void Initialize()
    {
        SubscribeLocalEvent<MimeryBookComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<MimeryBookComponent, Goobstation.Shared.Mimery.MimeryBookDoAfterEvent>(OnDoAfter);
    }


    private void OnUse(Entity<MimeryBookComponent> ent, ref UseInHandEvent args)
    {
        AttmeptLearn(ent, args);
    }
    private void OnDoAfter(Entity<MimeryBookComponent> ent, ref Goobstation.Shared.Mimery.MimeryBookDoAfterEvent args)
    {
        if (args.Handled ||  args.Cancelled)
            return;

        if (TryComp<MimePowersComponent>(args.Args.User, out var mimePowersComponent))
        {
            if (mimePowersComponent.VowBroken)
            {
                _popupSystem.PopupEntity(Loc.GetString("mimery-book-fail-vow-broken-popup"), args.Args.User, args.Args.User);
                return;
            }
            EnsureComp<MimeryPowersComponent>(args.Args.User);
        }
        else
            _popupSystem.PopupEntity(Loc.GetString("mimery-book-fail-read-popup"), args.Args.User, args.Args.User);

        args.Handled = true;
    }

    private void AttmeptLearn(Entity<MimeryBookComponent> ent, UseInHandEvent args)
    {
        var DoAfterEventArgs = new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.LearnTime,
            new Goobstation.Shared.Mimery.MimeryBookDoAfterEvent(),
            ent,
            target: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnDropItem = true,
            NeedHand = true,
            MultiplyDelay = true,
        };

        _doAfter.TryStartDoAfter(DoAfterEventArgs);
    }

}
