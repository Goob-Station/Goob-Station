using Content.Shared._Shitcode.Heretic.Systems;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Shitcode.Heretic.Beam;

public sealed class ContinuousBeamSystem : SharedContinuousBeamSystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay.AddOverlay(new ContinuousBeamOverlay(EntityManager, _prototype, _timing));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlay.RemoveOverlay<ContinuousBeamOverlay>();
    }
}
