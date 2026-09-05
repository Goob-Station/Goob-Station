using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Gangwars;

/// <summary>
/// Visibility is gated inside the overlay
/// </summary>
public sealed class ClientGangTerritorySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlayManager.AddOverlay(new GangTerritoryOverlay(EntityManager));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayManager.RemoveOverlay<GangTerritoryOverlay>();
    }
}
