using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Interaction;

namespace Content.Server._Goobstation._Pirates.Pirates.Siphon;

public sealed partial class ResourceSiphonSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceSiphonComponent, AfterInteractEvent>(OnInteract);
    }

    private void OnInteract(Entity<ResourceSiphonComponent> ent, ref AfterInteractEvent args)
    {
        // TODO do a confirmation check
        ActivateSiphon(ent);
    }

    public void ActivateSiphon(Entity<ResourceSiphonComponent> ent)
    {
        ent.Comp.Active = true;

        _chat.DispatchGlobalAnnouncement(Loc.GetString("data-siphon-activated"), "Priority", colorOverride: Color.Red);
    }
}
