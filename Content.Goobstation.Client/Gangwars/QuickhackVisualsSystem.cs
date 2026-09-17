using Content.Goobstation.Shared.Gangwars.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Gangwars;

public sealed class QuickhackVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QuickhackComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, QuickhackComponent comp, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.LayerSetRsiState((uid, sprite), 0, comp.State == QuickhackState.Firing ? "fire" : "icon");
    }
}
