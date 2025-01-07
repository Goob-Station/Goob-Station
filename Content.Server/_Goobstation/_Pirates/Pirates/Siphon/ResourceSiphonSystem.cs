using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Interaction;

namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

public sealed partial class ResourceSiphonSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceSiphonComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<ResourceSiphonComponent, AfterInteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteract(Entity<ResourceSiphonComponent> ent, ref AfterInteractEvent args)
    {


        // TODO do a confirmation check
        //ActivateSiphon(ent);
    }

    private void OnInteractUsing(Entity<ResourceSiphonComponent> ent, ref AfterInteractUsingEvent args)
    {
        // todo add money
    }

    public void ActivateSiphon(Entity<ResourceSiphonComponent> ent)
    {
        ent.Comp.Active = true;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("data-siphon-activated"), "Priority", colorOverride: Color.Red);
    }
}
