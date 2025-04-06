using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;

namespace Content.Shared._Shitcode.Mimery;

public sealed class MimeryBookSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

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

        Logger.Debug("granted action");
        _actions.AddAction(args.Args.User, "ActionMimeryWall");
        EnsureComp<FingerGunComponent>(args.Args.User);


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
