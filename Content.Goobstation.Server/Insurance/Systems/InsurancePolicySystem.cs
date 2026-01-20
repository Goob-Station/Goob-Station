using Content.Goobstation.Server.Insurance.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.Insurance.Systems;

public sealed partial class InsurancePolicySystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InsurancePolicyComponent, AfterInteractEvent>(AfterInteract);
    }

    public void Insure(EntityUid target, EntityUid owner, InsurancePolicyComponent comp)
    {
        var insurance = EnsureComp<InsuranceComponent>(target);
        insurance.Beneficiary = owner;
        insurance.Policy = comp.Policy;
    }

    private void AfterInteract(Entity<InsurancePolicyComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target == null || !args.CanReach)
            return;

        if (!_whitelist.IsValid(ent.Comp.Policy.ValidEntities, args.Target.Value))
        {
            _popup.PopupEntity(Loc.GetString("insurance-not-insurable"), args.Target.Value, args.User);
            return;
        }

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("insurance-insure"), args.Target.Value, args.User);
        Insure(args.Target.Value, args.User, ent.Comp);
        QueueDel(args.Used);
    }
}
