// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Cuffs;
using Content.Server.Forensics;
using Content.Server.Humanoid;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.Teleportation;
using Content.Shared.Cuffs.Components;
using Content.Shared.Forensics;
using Content.Shared.Forensics.Components;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using System.Numerics;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Server.IdentityManagement;
using Content.Shared.DetailExaminable;
using Content.Shared.DoAfter;
using Content.Shared.Ensnaring;
using Content.Shared.Ensnaring.Components;
using Content.Shared.Implants.Components;
using Content.Shared.Polymorph;
using Content.Shared.Store.Components;
using Content.Shared.Teleportation;
using Content.Shared.Stunnable;

namespace Content.Server.Implants;
public sealed class SubdermalImplantSystem : SharedSubdermalImplantSystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StoreComponent, ImplantRelayEvent<AfterInteractUsingEvent>>(OnStoreRelay);

        SubscribeLocalEvent<ImplantedComponent, PolymorphedEvent>(OnPolymorphed); // goob edit
    }

    // todo goobstation move this to goobmod probably
    // goob edit - implants now transfer on polymorph
    private void OnPolymorphed(Entity<ImplantedComponent> ent, ref PolymorphedEvent args)
    {
        // copy it to prevent collection modification error
        var implants = new List<EntityUid>(ent.Comp.ImplantContainer.ContainedEntities);
        foreach (var implant in implants)
        {
            if (!TryComp<SubdermalImplantComponent>(implant, out var sic))
                continue;

            ForceImplant(args.NewEntity, implant, sic);
        }
    }
    // goob edit end


    private void OnStoreRelay(EntityUid uid, StoreComponent store, ImplantRelayEvent<AfterInteractUsingEvent> implantRelay)
    {
        var args = implantRelay.Event;

        if (args.Handled)
            return;

        // can only insert into yourself to prevent uplink checking with renault
        if (args.Target != args.User)
            return;

        if (!TryComp<CurrencyComponent>(args.Used, out var currency))
            return;

        // same as store code, but message is only shown to yourself
        if (!_store.TryAddCurrency((args.Used, currency), (uid, store)))
            return;

        args.Handled = true;
        var msg = Loc.GetString("store-currency-inserted-implant", ("used", args.Used));
        _popup.PopupEntity(msg, args.User, args.User);
    }
}
