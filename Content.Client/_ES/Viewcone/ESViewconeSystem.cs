using Content.Client._ES.Viewcone.Overlays;
using Content.Shared._ES.Viewcone;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Physics;
using Robust.Shared.Player;

namespace Content.Client._ES.Viewcone;

public sealed class ESViewconeSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    private ESViewconeConeOverlay _coneOverlay = default!;

    // slightly balls state management, but
    // done so we don't have to requery within the same frame
    // this is always cleared at the end of resetting alpha, so
    // it is the least thread safe code of all time obviously. but rendering not threaded. so
    // we can abuse the fact that the overlays will always draw sequentially in the order we expect, and
    // one wont start rendering in the middle of rendering another
    [Access(typeof(ESViewconeSetAlphaOverlay), typeof(ESViewconeResetAlphaOverlay))]
    public List<(Entity<SpriteComponent> ent, float baseAlpha)> CachedOccludables = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESViewconeComponent, ComponentInit>(OnConeManInit);
        SubscribeLocalEvent<ESViewconeComponent, ComponentShutdown>(OnConeManShutdown);

        SubscribeLocalEvent<ESViewconeComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<ESViewconeComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<ESViewconeComponent, ViewconeUpdateEvent>(OnViewconeUpdate);

        _coneOverlay = new();
        CachedOccludables = new(128);
    }

    private void OnPlayerAttached(Entity<ESViewconeComponent> entity, ref LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_coneOverlay);
    }

    private void OnPlayerDetached(Entity<ESViewconeComponent> entity, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_coneOverlay);
        ResetOccludedAlpha();
    }

    private void OnConeManInit(Entity<ESViewconeComponent> entity, ref ComponentInit args)
    {
        if (_playerManager.LocalSession?.AttachedEntity == entity.Owner)
            _overlayMan.AddOverlay(_coneOverlay);
    }

    private void OnConeManShutdown(Entity<ESViewconeComponent> entity, ref ComponentShutdown args)
    {
        if (_playerManager.LocalSession?.AttachedEntity == entity.Owner)
        {
            _overlayMan.RemoveOverlay(_coneOverlay);
            ResetOccludedAlpha();
        }
    }

    private void ResetOccludedAlpha()
    {
        var query = AllEntityQuery<ESViewconeOccludableComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite))
                continue;

            sprite.Color = sprite.Color.WithAlpha(comp.BaseAlpha);
        }
    }

    private void OnViewconeUpdate(Entity<ESViewconeComponent> entity, ref ViewconeUpdateEvent args)
    {
        UpdateViewcone(entity);
    }

    public void UpdateViewcone(Entity<ESViewconeComponent> entity)
    {

    }
}

public sealed class ViewconeUpdateEvent : EntityEventArgs
{
    public readonly EntityUid Entity;

    public ViewconeUpdateEvent(EntityUid entity)
    {
        Entity = entity;
    }
}
