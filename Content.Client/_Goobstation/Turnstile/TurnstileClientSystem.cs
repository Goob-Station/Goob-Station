using Content.Shared._Goobstation.Turnstile;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._Goobstation.Turnstile;

/// <inheritdoc/>
public sealed class TurnstileClientSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<StartTurnstileEvent>(OnTurnstileStart);
        SubscribeNetworkEvent<BadTurnstileEvent>(BadTurnstile);

    }
    private void BadTurnstile(BadTurnstileEvent args)
    {
        var uid = GetEntity(args.Uid);
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        sprite.LayerSetVisible(0,false);
        sprite.LayerSetVisible(1,true);
        Timer.Spawn(1000,
            () =>
            {
                sprite.LayerSetVisible(1,false);
                sprite.LayerSetVisible(0,true);
            });
    }
    private void OnTurnstileStart(StartTurnstileEvent args)
    {
        var uid = GetEntity(args.Uid);
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        sprite.LayerSetVisible(0,false);
        sprite.LayerSetVisible(2,true);
        Timer.Spawn(1000,
            () =>
            {
                sprite.LayerSetVisible(2,false);
                sprite.LayerSetVisible(0,true);
            });
    }
}
