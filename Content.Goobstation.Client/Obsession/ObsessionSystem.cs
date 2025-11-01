using Content.Goobstation.Shared.Obsession;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.Obsession;

public sealed class ObsessionSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private ObsessionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();

        SubscribeLocalEvent<ObsessedComponent, ComponentInit>(OnObsessedInit);
        SubscribeLocalEvent<ObsessedComponent, ComponentShutdown>(OnObsessedShutdown);
        SubscribeLocalEvent<ObsessedComponent, LocalPlayerAttachedEvent>(OnObsessedAttached);
        SubscribeLocalEvent<ObsessedComponent, LocalPlayerDetachedEvent>(OnObsessedDetached);

        SubscribeLocalEvent<ObsessedComponent, AfterAutoHandleStateEvent>(OnAutoHandleState);
    }

    private void OnObsessedInit(Entity<ObsessedComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnObsessedShutdown(Entity<ObsessedComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity == ent.Owner)
            _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnObsessedAttached(Entity<ObsessedComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
        _overlay.ObsessionId = ent.Comp.TargetId;
    }

    private void OnObsessedDetached(Entity<ObsessedComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnAutoHandleState(Entity<ObsessedComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_player.LocalEntity == ent.Owner)
        {
            _overlay.MaxBlur = ent.Comp.SanityLossStage;
            _overlay.ObsessionId = ent.Comp.TargetId;
        }
    }
}
