// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Shared.Implants;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Implants.Components;
using Content.Shared.Polymorph;
using Content.Shared.Store.Components;

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

            var implantEnt = new Entity<SubdermalImplantComponent>(ent, sic);

            ForceImplant(args.NewEntity, implantEnt!);
        }
    }
    // goob edit end


    // TODO: This shouldn't be in the SubdermalImplantSystem
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
