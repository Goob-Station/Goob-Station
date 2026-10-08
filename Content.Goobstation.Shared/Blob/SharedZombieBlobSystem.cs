// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Interaction;
using Content.Shared.Nuke;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.Blob;

public abstract class SharedZombieBlobSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZombieBlobComponent, ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<BoundUserInterfaceMessageAttempt>(OnBoundUserInterface, after: [typeof(SharedInteractionSystem)]);
    }

    private void OnBoundUserInterface(ref BoundUserInterfaceMessageAttempt args)
    {
        if(
            args.Cancelled ||
            !TryComp<ActivatableUIComponent>(args.Target, out var uiComp) ||
            !HasComp<ZombieBlobComponent>(args.Actor))
            return;

        if (args.UiKey is NukeUiKey)
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