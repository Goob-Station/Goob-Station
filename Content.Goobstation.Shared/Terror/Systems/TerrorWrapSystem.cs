using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Terror.Systems;

public sealed class TerrorWrapSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ISharedAdminLogManager _admin = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorWrapComponent, TerrorWrapEvent>(OnTryWrap);

        SubscribeLocalEvent<TerrorWrapComponent, TerrorWrapDoAfterEvent>(OnWrapDoAfter);

    }

    private void OnTryWrap(Entity<TerrorWrapComponent> ent, ref TerrorWrapEvent args)
    {

        var target = args.Target;
        var uid = ent.Owner;
        //_popup.PopupPredicted(Loc.GetString("terror-wrap", ("user", ent.Owner), ("target", args.Target)), ent.Owner, args.Target, PopupType.LargeCaution);

        // TO DO: Change this FTL
        if (!_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("They must be dead to be wrapped."), uid, uid);
            return;
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            ent.Comp.DoAfter,
            new TerrorWrapDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnDamage = false,
            BreakOnMove = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        args.Handled = true;
    }

    private void OnWrapDoAfter(Entity<TerrorWrapComponent> ent, ref TerrorWrapDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target is not { } target)
        {
            return;
        }

        // TO DO: Change the cocoon into a container of the body, and husk the person inside it.
        PredictedSpawnAtPosition(ent.Comp.CocoonProto, Transform(target).Coordinates);
        QueueDel(target);
        _admin.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent.Owner)} cocooned {ToPrettyString(target)} as a Terror Spider, husking them in the process.");
        args.Handled = true;
    }
}
