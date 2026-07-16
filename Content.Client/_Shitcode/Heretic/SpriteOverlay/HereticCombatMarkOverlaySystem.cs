using Content.Shared.Heretic;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic.SpriteOverlay;

public sealed class HereticCombatMarkOverlaySystem : SpriteOverlaySystem<HereticCombatMarkComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HereticCombatMarkComponent, AfterAutoHandleStateEvent>((uid, comp, _) =>
            AddOverlay(uid, comp));
    }

    protected override int? GetLayerIndex(Entity<SpriteComponent> ent, HereticCombatMarkComponent comp)
    {
        return comp.Path == "Cosmos" ? 0 : null; // Cosmos mark should be behind the sprite
    }

    protected override void UpdateOverlayLayer(Entity<SpriteComponent> ent,
        HereticCombatMarkComponent comp,
        int layer,
        EntityUid? source = null)
    {
        base.UpdateOverlayLayer(ent, comp, layer, source);
        var state = comp.Path.ToLower();
        Sprite.LayerSetRsiState(ent.AsNullable(), layer, state);
    }
}
