using Content.Client._ES.Viewcone.Overlays;
using Content.Shared._ES.Viewcone;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Physics;
using Robust.Shared.Player;

namespace Content.Client._ES.Viewcone;

/// <summary>
///     Handles adding and removing the viewcone overlays, as well as ferrying data between them
/// </summary>
public sealed class ESViewconeOverlayManagementSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    private ESViewconeConeOverlay _coneOverlay = default!;
    private ESViewconeSetAlphaOverlay _setAlphaOverlay = default!;
    private ESViewconeResetAlphaOverlay _resetAlphaOverlay = default!;

    // slightly balls state management, but
    // done so we don't have to requery within the same frame
    // this is always cleared at the end of resetting alpha
    // it is the least thread safe code of all time obviously. but rendering not threaded. so
    // we can abuse the fact that the overlays will always draw sequentially in the order we expect, and
    // one wont start rendering in the middle of rendering another
    [Access(typeof(ESViewconeSetAlphaOverlay), typeof(ESViewconeResetAlphaOverlay))]
    public List<(Entity<SpriteComponent> ent, float baseAlpha)> CachedBaseAlphas = new(128);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESViewconeComponent, ComponentInit>(OnConeManInit);
        SubscribeLocalEvent<ESViewconeComponent, ComponentShutdown>(OnConeManShutdown);

        SubscribeLocalEvent<ESViewconeComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ESViewconeComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _coneOverlay = new();
        _setAlphaOverlay = new();
        _resetAlphaOverlay = new();
    }

    private void OnPlayerAttached(Entity<ESViewconeComponent> entity, ref LocalPlayerAttachedEvent args)
    {
        AddOverlays();
    }

    private void OnPlayerDetached(Entity<ESViewconeComponent> entity, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlays();
    }

    private void OnConeManInit(Entity<ESViewconeComponent> entity, ref ComponentInit args)
    {
        if (_playerManager.LocalSession?.AttachedEntity == entity.Owner)
            AddOverlays();
    }

    private void OnConeManShutdown(Entity<ESViewconeComponent> entity, ref ComponentShutdown args)
    {
        if (_playerManager.LocalSession?.AttachedEntity == entity.Owner)
        {
            RemoveOverlays();
        }
    }

    private void AddOverlays()
    {
        _overlayMan.AddOverlay(_coneOverlay);
        _overlayMan.AddOverlay(_setAlphaOverlay);
        _overlayMan.AddOverlay(_resetAlphaOverlay);
    }

    private void RemoveOverlays()
    {
        _overlayMan.RemoveOverlay(_coneOverlay);
        _overlayMan.RemoveOverlay(_setAlphaOverlay);
        _overlayMan.RemoveOverlay(_resetAlphaOverlay);
    }
}
