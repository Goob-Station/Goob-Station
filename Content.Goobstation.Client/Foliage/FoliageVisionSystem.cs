// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Goobstation.Shared.Foliage;

namespace Content.Goobstation.Client.Foliage;

public sealed class FoliageVisionSystem : EntitySystem
{
    private const int HighDrawDepth = 10;
    private const int LowDrawDepth = -5;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    private bool _enabled;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FoliageIgnoringVisionComponent, LocalPlayerAttachedEvent>((_, _, args) => OnLocalPlayerAttached(args));
        SubscribeLocalEvent<FoliageIgnoringVisionComponent, LocalPlayerDetachedEvent>((_, _, args) => OnLocalPlayerDetached(args));
        SubscribeLocalEvent<FoliageIgnoringVisionComponent, ComponentStartup>((uid, _, args) => OnPlayerComponentStartup(uid, args));
        SubscribeLocalEvent<FoliageIgnoringVisionComponent, ComponentShutdown>((uid, _, args) => OnPlayerComponentShutdown(uid, args));
        SubscribeLocalEvent<HideableFoliageComponent, ComponentStartup>((uid, _, args) => OnKudzuComponentStartup(uid, args));
        SubscribeLocalEvent<HideableFoliageComponent, ComponentShutdown>((uid, _, args) => OnKudzuComponentShutdown(uid, args));
    }

    // Attaches detaches
    private void OnLocalPlayerAttached(LocalPlayerAttachedEvent args)
    {
        UpdatePlayerFoliageIgnoringVision();
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent args)
    {
        UpdatePlayerFoliageIgnoringVision();
    }

    // Player Startup/Shutdown
    private void OnPlayerComponentStartup(EntityUid uid, ComponentStartup args)
    {
        if (uid == _player.LocalEntity)
            UpdatePlayerFoliageIgnoringVision();
    }

    private void OnPlayerComponentShutdown(EntityUid uid, ComponentShutdown args)
    {
        if (uid == _player.LocalEntity)
            UpdatePlayerFoliageIgnoringVision();
    }

    // Kudzu Startup/Shutdown
    private void OnKudzuComponentStartup(EntityUid uid, ComponentStartup args)
    {
        UpdateFoliageDrawDepth(uid);
    }

    private void OnKudzuComponentShutdown(EntityUid uid, ComponentShutdown args)
    {
        UpdateFoliageDrawDepth(uid);
    }

    private void RefreshEveryPieceOfFoliage()
    {
        var query = EntityQueryEnumerator<HideableFoliageComponent, SpriteComponent>();

        while (query.MoveNext(out var uid, out _, out var sprite))
        {
            UpdateFoliageDrawDepth(uid, sprite);
        }
    }

    private void UpdateFoliageDrawDepth(EntityUid uid, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite, false))
            return;

        var drawDepth = _enabled
            ? DrawDepth.Default + LowDrawDepth
            : DrawDepth.Default + HighDrawDepth;
        _sprite.SetDrawDepth((uid, sprite), drawDepth);
    }

    private void UpdatePlayerFoliageIgnoringVision()
    {
        var previousEnabled = _enabled;
        var attached = _player.LocalEntity;
        var shouldHaveFoliageIgnoringVision = attached != null && HasComp<FoliageIgnoringVisionComponent>(attached.Value);
        _enabled = shouldHaveFoliageIgnoringVision;
        if (previousEnabled != _enabled)
            RefreshEveryPieceOfFoliage();
    }
}
