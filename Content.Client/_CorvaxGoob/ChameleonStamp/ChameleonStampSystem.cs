using Content.Shared._CorvaxGoob.ChameleonStamp;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client._CorvaxGoob.ChameleonStamp;

public sealed partial class ChameleonStampSystem : SharedChameleonStampSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ChameleonStampComponent, AfterAutoHandleStateEvent>(HandleState);
        UpdatePresets();
    }

    private void HandleState(Entity<ChameleonStampComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(entity);
    }

    public void UpdateVisuals(Entity<ChameleonStampComponent> entity)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite)
            || !_proto.TryIndex<EntityPrototype>(entity.Comp.SelectedStampSpritePrototype, out var proto)
            || !proto.TryGetComponent<SpriteComponent>(out var presetSprite))
            return;

        sprite.CopyFrom(presetSprite);
    }
}
