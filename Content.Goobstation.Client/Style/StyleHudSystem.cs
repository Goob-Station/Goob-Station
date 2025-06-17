using Content.Goobstation.Common.Style;
using Content.Goobstation.Shared.Style;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Style;

public sealed class StyleHudSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlays = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private StyleHudOverlay? _overlay;
    private EntityUid? _currentEntity;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StyleCounterComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StyleCounterComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeNetworkEvent<StyleHudUpdateEvent>(OnHudUpdate);
    }

    private void OnHudUpdate(StyleHudUpdateEvent ev)
    {
        if (_player.LocalEntity is not { } player)
            return;

        if (!TryComp<StyleCounterComponent>(player, out var style))
            return;

        style.Rank = ev.Rank;
        style.CurrentMultiplier = ev.Multiplier;
        style.RecentEvents = new List<string>(ev.RecentEvents);
        Dirty(player, style);
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        if (HasComp<StyleCounterComponent>(ev.Entity))
        {
            _currentEntity = ev.Entity;
            EnsureOverlay();
        }
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        if (_currentEntity == ev.Entity)
        {
            _currentEntity = null;
            RemoveOverlay();
        }
    }

    private void OnStartup(EntityUid uid, StyleCounterComponent component, ComponentStartup args)
    {
        if (_player.LocalEntity == uid)
        {
            _currentEntity = uid;
            EnsureOverlay();
        }
    }

    private void OnShutdown(EntityUid uid, StyleCounterComponent component, ComponentShutdown args)
    {
        if (_player.LocalEntity == uid)
        {
            _currentEntity = null;
            RemoveOverlay();
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var player = _player.LocalEntity;
        if (player != null && _currentEntity != player && HasComp<StyleCounterComponent>(player.Value))
        {
            _currentEntity = player.Value;
            EnsureOverlay();
        }
        else if ((player == null || !HasComp<StyleCounterComponent>(player.Value)) && _currentEntity != null)
        {
            _currentEntity = null;
            RemoveOverlay();
        }
    }

    private void EnsureOverlay()
    {
        if (_overlay != null)
            return;

        _overlay = new StyleHudOverlay(_player);
        _overlays.AddOverlay(_overlay);
    }

    private void RemoveOverlay()
    {
        if (_overlay == null)
            return;

        _overlays.RemoveOverlay(_overlay);
        _overlay = null;
    }
}
