using Content.Shared._Goobstation.Heretic.Components;

namespace Content.Client._Shitcode.Heretic.SpriteOverlay;

public sealed class EntropicPlumeOverlaySystem : SpriteOverlaySystem<EntropicPlumeAffectedComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }
}
