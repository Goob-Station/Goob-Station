using Content.Goobstation.Common.MartialArts;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;

namespace Content.Server._Shitcode.Mimery;

public sealed class MimeryBookSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<MimeryBookComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<MimeryBookComponent, MimeryBookDoAfterEvent>(OnDoAfter);
    }


    private void OnUse(Entity<MimeryBookComponent> ent, ref UseInHandEvent args)
    {
        AttmeptLearn(ent, args);
    }
    private void OnDoAfter(Entity<MimeryBookComponent> ent, ref MimeryBookDoAfterEvent args)
    {
        if (args.Handled ||  args.Cancelled)
            return;
        args.Handled = true;

        EnsureComp<MimeryPowersComponent>(args.Args.User); // i gotta make the sv add fingerguncomponent, its serversided
    }

    private void AttmeptLearn(Entity<MimeryBookComponent> ent, UseInHandEvent args)
    {
        var DoAfterEventArgs = new DoAfterArgs(EntityManager,
            args.User,
            ent.Comp.LearnTime,
            new MimeryBookDoAfterEvent(),
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
