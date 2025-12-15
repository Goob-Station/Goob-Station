using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Insurance;

public abstract class SharedInsurancePolicySystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InsurancePolicyComponent, AfterInteractEvent>(AfterInteract);
    }

    private void AfterInteract(Entity<InsurancePolicyComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach)
            return;

        if (!_whitelist.IsValid(ent.Comp.ValidEntities, args.Target.Value))
        {
            _popup.PopupCursor(Loc.GetString("insurance-not-insurable"));
            return;
        }

        var proto = Prototype(args.Target.Value);
        if (proto == null)
        {
            _popup.PopupCursor(Loc.GetString("insurance-not-insurable"));
            return;
        }

        args.Handled = true;

        _popup.PopupCursor(Loc.GetString("insurance-insure"));

        if (_net.IsServer)
        {
            // apparently, initializing a list with [...] is illegal on the client
            // so we have to do it this way, because this assembly is loaded on the client
            List<EntProtoId> compensationItems = new() { proto };
            if (ent.Comp.ExtraCompensationItems != null)
                compensationItems.AddRange(ent.Comp.ExtraCompensationItems);
            Insure(args.Target.Value, args.User, compensationItems);
        }

        QueueDel(args.Used);
    }

    // no-op on client
    public abstract void Insure(EntityUid target, EntityUid owner, List<EntProtoId> compensationItems);
}
