using Content.Goobstation.Shared.Emag.Components;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Emag.Systems;

public sealed partial class CleanEmagSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EmagSystem _emag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CleanEmagComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<EmaggedComponent, CleaningEmaggedDeviceDoAfterEvent>(OnCleaningEmaggedDevice);
    }

    private void OnAfterInteract(Entity<CleanEmagComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || args.Handled)
            return;

        if (!_emag.CheckAnyFlag(args.Target, EmagType.Jestographic))
            return;

        var doAfter = new DoAfterArgs(EntityManager, args.User, ent.Comp.CleanDuration, new CleaningEmaggedDeviceDoAfterEvent(), args.Target, args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnHandChange = true
        };

        _doAfter.TryStartDoAfter(doAfter);

        _popup.PopupPredicted(Loc.GetString("emag-cleaning", ("device", args.Target)), args.User, args.User);

        args.Handled = true;
    }

    private void OnCleaningEmaggedDevice(Entity<EmaggedComponent> ent, ref CleaningEmaggedDeviceDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        // Only remove the jestographic flag
        ent.Comp.EmagType &= ~EmagType.Jestographic;
        Dirty(ent);

        if (ent.Comp.EmagType == EmagType.None)
            RemCompDeferred<EmaggedComponent>(ent.Owner);

        var ev = new EmagCleanedEvent(args.User);
        RaiseLocalEvent(ent.Owner, ref ev);

        args.Handled = true;
    }
}
