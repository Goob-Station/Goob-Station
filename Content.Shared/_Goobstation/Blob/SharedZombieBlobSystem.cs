using Content.Shared._Goobstation.Blob.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._Goobstation.Blob;

public abstract class SharedZombieBlobSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZombieBlobComponent, ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<BoundUserInterfaceMessageAttempt>(OnBoundUserInterface, after: [typeof(SharedInteractionSystem)]);
    }

    private void OnBoundUserInterface(BoundUserInterfaceMessageAttempt args)
    {
        if(
            args.Cancelled ||
            !TryComp<ActivatableUIComponent>(args.Target, out var uiComp) ||
            !HasComp<ZombieBlobComponent>(args.Actor))
            return;

        if(uiComp.RequiresComplex)
            args.Cancel();
    }

    private void OnAttemptShoot(Entity<ZombieBlobComponent> ent, ref ShotAttemptedEvent args)
    {
        if(ent.Comp.CanShoot)
            return;

        _popup.PopupClient(Loc.GetString("blob-no-using-guns-popup"), ent, ent);
        args.Cancel();
    }
}
