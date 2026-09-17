using Content.Shared.Power.EntitySystems;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Wall;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Silicons.StationAi;

public sealed partial class StationAiOverlay
{
    private static readonly ProtoId<ShaderPrototype> AiMachineViewShader = "AiMachineView";

    private readonly HashSet<Entity<StationAiWhitelistComponent>> _aiMachineViewCandidates = new();

    /// <summary>
    /// Renders with shader the sprite of every anchored, powered, AI-whitelisted machine that is hidden by the static.
    /// </summary>
    private void DrawAiMachineView(in OverlayDrawArgs args, DrawingHandleWorld worldHandle, EntityUid gridUid, MapGridComponent grid)
    {
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var lookup = _entManager.System<EntityLookupSystem>();
        var power = _entManager.System<SharedPowerReceiverSystem>();
        var sprites = _entManager.System<SpriteSystem>();
        var xforms = _entManager.System<SharedTransformSystem>();
        var maps = _entManager.System<SharedMapSystem>();

        _aiMachineViewCandidates.Clear();
        lookup.GetEntitiesIntersecting(args.MapId, args.WorldBounds, _aiMachineViewCandidates, LookupFlags.Static | LookupFlags.Sundries | LookupFlags.Approximate);

        var shader = _proto.Index(AiMachineViewShader).Instance();

        foreach (var ent in _aiMachineViewCandidates)
        {
            if (!ent.Comp.Enabled)
                continue;

            if (!_entManager.TryGetComponent<TransformComponent>(ent.Owner, out var entXform)
                || (!entXform.Anchored && !_entManager.HasComponent<WallMountComponent>(ent.Owner)) // WallMount fixtures (e.g. signal switches) aren't Anchored
                || entXform.GridUid != gridUid)
                continue;

            if (!power.IsPowered(ent.Owner))
                continue;

            var tile = maps.LocalToTile(gridUid, grid, entXform.Coordinates);
            if (_visibleTiles.Contains(tile))
                continue; //don't draw already visible

            if (!_entManager.TryGetComponent<SpriteComponent>(ent.Owner, out var sprite) || !sprite.Visible)
                continue;

            var (worldPos, worldRot) = xforms.GetWorldPositionRotation(entXform);

            worldHandle.UseShader(shader);
            sprites.RenderSprite((ent.Owner, sprite), worldHandle, eye.Rotation, worldRot, worldPos);
        }
    }
}
