using Content.Client.Flash;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Timing;


namespace Content.Client._EE.Shadowling;

public sealed class EnthrallSystem : SharedEnthrallSystem
{
    private EnthrallOverlay _overlay = default!;

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<ThrallComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ThrallComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, ThrallComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayManager.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, ThrallComponent component, LocalPlayerDetachedEvent args)
    {
        _overlayManager.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, ThrallComponent component, ComponentInit init)
    {
        if (_playerManager.LocalEntity == uid)
        {
            if (EntityManager.HasComponent<LesserShadowlingComponent>(uid))
                return;

            _overlay.ReceiveEnthrall(5f);
            _overlayManager.AddOverlay(_overlay);
        }
    }
}
