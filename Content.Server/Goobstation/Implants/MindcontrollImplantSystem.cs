using Content.Server.Implants.Components;
using Content.Shared.Implants;
using Robust.Shared.Containers;
using Content.Shared.Mindcontroll;
using Content.Server.Mindcontroll;
using Content.Shared.Implants.Components;
using Content.Shared.Interaction;
using Content.Shared.Hands.Components;

namespace Content.Server.Implants;
public sealed class MindcontrollImplantSystem : EntitySystem
{
    [Dependency] private readonly MindcontrollSystem _mindcontroll = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindcontrollImplantComponent, EntGotRemovedFromContainerMessage>(OnRemove); //implant gets removed, remove traitor
        SubscribeLocalEvent<MindcontrollImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<MindcontrollImplantComponent, EntGotInsertedIntoContainerMessage>(OnInsert);
    }
    private void OnImplant(EntityUid uid, MindcontrollImplantComponent component, ImplantImplantedEvent args) //called after implanted ?
    {
        if (component.ImplanterUid != null)
        {
            component.HolderUid = Transform(component.ImplanterUid.Value).ParentUid;
        }
        if (args.Implanted != null)
            EnsureComp<MindcontrollComponent>(args.Implanted.Value);

        component.ImplanterUid = null;
        if (args.Implanted == null)
            return;
        if (!TryComp<MindcontrollComponent>(args.Implanted.Value, out var implanted))
            return;
        implanted.Master = component.HolderUid;
        _mindcontroll.Start(args.Implanted.Value, implanted);
    }
    private void OnInsert(EntityUid uid, MindcontrollImplantComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == "implanter_slot")  //being inserted in a implanter.
        {
            component.ImplanterUid = args.Container.Owner;    //save Implanter uid
            component.HolderUid = null;
        }
    }
    private void OnRemove(EntityUid uid, MindcontrollImplantComponent component, EntGotRemovedFromContainerMessage args)
    {

        if (args.Container.ID == "implant") //when implant is removed
        {
            if (HasComp<MindcontrollComponent>(args.Container.Owner))
                RemComp<MindcontrollComponent>(args.Container.Owner);
        }
    }
}
