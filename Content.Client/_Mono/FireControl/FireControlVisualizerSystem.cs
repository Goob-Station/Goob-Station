// Copyright Rane (elijahrane@gmail.com) 2025
// All rights reserved. Relicensed under AGPL with permission

using System.Numerics;
using Content.Shared._Mono.FireControl;
using Robust.Client.Graphics;
using Robust.Shared.Enums;

namespace Content.Client._Mono.FireControl;

/// <summary>
/// Client-side system that visualizes firing directions for debug purposes
/// </summary>
public sealed class FireControlVisualizerSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private readonly Dictionary<EntityUid, VisualizationData> _activeVisualizations = new();

    public sealed class VisualizationData
    {
        public Dictionary<float, bool> Directions { get; }

        public VisualizationData(Dictionary<float, bool> directions)
        {
            Directions = directions;
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<FireControlVisualizationEvent>(OnFireControlVisualization);

        _overlayManager.AddOverlay(new FireControlOverlay(this, EntityManager));
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _overlayManager.RemoveOverlay<FireControlOverlay>();
    }

    private void OnFireControlVisualization(FireControlVisualizationEvent ev)
    {
        // Convert NetEntity back to EntityUid
        var entityUid = GetEntity(ev.Entity);

        if (ev.Enabled && ev.Directions != null)
        {
            // Enable or update visualization
            _activeVisualizations[entityUid] = new VisualizationData(ev.Directions);
        }
        else
        {
            // Disable visualization
            _activeVisualizations.Remove(entityUid);
        }
    }

    /// <summary>
    /// Gets the current active visualizations for the overlay to render
    /// </summary>
    public IReadOnlyDictionary<EntityUid, VisualizationData> GetVisualizations()
    {
        return _activeVisualizations;
    }

    /// <summary>
    /// Overlay that renders the fire direction visualization
    /// </summary>
    private sealed class FireControlOverlay : Overlay
    {
        private readonly FireControlVisualizerSystem _system;
        private readonly IEntityManager _entityManager;
        private readonly SharedTransformSystem _transformSystem;

        public override OverlaySpace Space => OverlaySpace.WorldSpace;

        public FireControlOverlay(FireControlVisualizerSystem system, IEntityManager entityManager)
        {
            _system = system;
            _entityManager = entityManager;
            _transformSystem = entityManager.System<SharedTransformSystem>();
        }

        protected override void Draw(in OverlayDrawArgs args)
        {
            var handle = args.WorldHandle;
            var visualizations = _system.GetVisualizations();

            foreach (var (uid, data) in visualizations)
            {
                if (!_entityManager.TryGetComponent(uid, out TransformComponent? transform))
                    continue;

                var position = _transformSystem.GetWorldPosition(transform);

                // Draw a small circle at the weapon position
                handle.DrawCircle(position, 0.3f, Color.Yellow, true);

                // Draw rays for each direction
                foreach (var (angle, canFire) in data.Directions)
                {
                    // Convert angle to radians
                    var radians = angle * Math.PI / 180;

                    // Calculate end point (25 units length)
                    var direction = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                    var endPoint = position + direction * 25f;

                    // Choose color based on whether firing is possible
                    var color = canFire ? Color.Green : Color.Red;

                    // Draw the ray (remove the thickness parameter)
                    handle.DrawLine(position, endPoint, color);
                }
            }
        }
    }
}
