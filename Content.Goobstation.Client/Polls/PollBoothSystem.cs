using System.Numerics;
using Content.Goobstation.Shared.Polls;
using Content.Shared.Interaction;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Polls;

public sealed class PollBoothSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly PollManager _polls = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private static readonly SpriteSpecifier AlertSprite =
        new SpriteSpecifier.Rsi(new ResPath("Structures/Storage/closet.rsi"), "cardboard_special");

    private bool _requestedPolls;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PollBoothComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<PollBoothComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PollBoothComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PollBoothComponent, MoveEvent>(OnMove);
        _polls.OnUnseenChanged += RefreshAll;
    }

    public override void Shutdown()
    {
        _polls.OnUnseenChanged -= RefreshAll;
        base.Shutdown();
    }

    private void OnActivate(Entity<PollBoothComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        _ui.GetUIController<PollUIController>().OpenWindow();
        args.Handled = true;
    }

    private void OnStartup(Entity<PollBoothComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((ent.Owner, sprite), PollBoothLayer.Alert, out _, false))
            return;

        var layer = _sprite.AddLayer((ent.Owner, sprite), AlertSprite);
        _sprite.LayerMapSet((ent.Owner, sprite), PollBoothLayer.Alert, layer);
        sprite.LayerSetShader(layer, "unshaded");
        _sprite.LayerSetVisible((ent.Owner, sprite), layer, _polls.HasUnseenPolls);
        UpdateAlertTransform((ent.Owner, sprite), layer);

        if (!_requestedPolls)
        {
            _requestedPolls = true;
            _polls.RequestActivePolls();
        }
    }

    private void OnShutdown(Entity<PollBoothComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent.Owner, sprite), PollBoothLayer.Alert, out var layer, false))
            return;

        _sprite.RemoveLayer((ent.Owner, sprite), layer);
    }

    private void OnMove(Entity<PollBoothComponent> ent, ref MoveEvent args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent.Owner, sprite), PollBoothLayer.Alert, out var layer, false))
            return;

        UpdateAlertTransform((ent.Owner, sprite), layer);
    }

    private void UpdateAlertTransform(Entity<SpriteComponent?> sprite, int layer)
    {
        var rot = Transform(sprite.Owner).LocalRotation;
        _sprite.LayerSetRotation(sprite, layer, -rot);
        _sprite.LayerSetOffset(sprite, layer, (-rot).RotateVec(new Vector2(0f, 1f)));
    }

    private void RefreshAll()
    {
        var hasUnseen = _polls.HasUnseenPolls;
        var query = EntityQueryEnumerator<PollBoothComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            if (_sprite.LayerMapTryGet((uid, sprite), PollBoothLayer.Alert, out var layer, false))
                _sprite.LayerSetVisible((uid, sprite), layer, hasUnseen);
        }
    }
}

public enum PollBoothLayer : byte
{
    Alert,
}
