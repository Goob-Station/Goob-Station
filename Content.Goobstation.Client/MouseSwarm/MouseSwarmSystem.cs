using Content.Goobstation.Shared.MouseSwarm;
using Robust.Client.GameObjects;
using Robust.Shared.Analyzers;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MouseSwarm;

public sealed partial class MouseSwarmSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    private const string LayerPrefix = "mouse-swarm-";

    [SubscribeLocalEvent]
    private void OnStartup(Entity<MouseSwarmComponent> ent, ref ComponentStartup args)
    {
        UpdateLayers(ent);
    }

    [SubscribeLocalEvent]
    private void OnHandleState(Entity<MouseSwarmComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateLayers(ent);
    }

    private void UpdateLayers(Entity<MouseSwarmComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var spriteEnt = (ent.Owner, sprite);
        for (var i = 0; _sprite.RemoveLayer(spriteEnt, LayerPrefix + i, false); i++) { }

        for (var i = 0; i < ent.Comp.Members.Count; i++)
        {
            var member = ent.Comp.Members[i];
            var index = _sprite.AddLayer(spriteEnt, new SpriteSpecifier.Rsi(member.Sprite, member.State));
            _sprite.LayerMapSet(spriteEnt, LayerPrefix + i, index);
            _sprite.LayerSetOffset(spriteEnt, index, member.Offset);
        }
    }
}
