// SPDX-FileCopyrightText: 2026 Raze500
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SeeOverLayer;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.SeeOverLayer;

/// <summary>
/// Adjusts the draw depth of entities with <see cref="SeeOverLayerVisualsComponent"/>
/// based on whether the local player's mob has <see cref="SeeOverLayerComponent"/>
/// with a matching layer key.
///
/// This system is intentionally generic: it does not know about kudzu, Diona,
/// or any other specific content. Species and objects opt in via YAML.
/// </summary>
public sealed class SeeOverLayerSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SeeOverLayerVisualsComponent, ComponentStartup>(OnVisualsStartup);
        SubscribeLocalEvent<SeeOverLayerVisualsComponent, ComponentShutdown>(OnVisualsShutdown);
        SubscribeLocalEvent<SeeOverLayerVisualsComponent, AppearanceChangeEvent>(OnAppearanceChanged);

        SubscribeLocalEvent<SeeOverLayerComponent, ComponentStartup>(OnViewerStartup);
        SubscribeLocalEvent<SeeOverLayerComponent, ComponentShutdown>(OnViewerShutdown);

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    // --- Obstacle entity events ---

    private void OnVisualsStartup(Entity<SeeOverLayerVisualsComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        UpdateDrawDepth((ent.Owner, sprite), ent.Comp, GetLocalPlayerLayers());
    }

    private void OnVisualsShutdown(Entity<SeeOverLayerVisualsComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _sprite.SetDrawDepth((ent.Owner, sprite), ent.Comp.NormalDrawDepth);
    }

    private void OnAppearanceChanged(Entity<SeeOverLayerVisualsComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        UpdateDrawDepth((ent.Owner, args.Sprite), ent.Comp, GetLocalPlayerLayers());
    }

    // --- Viewer mob events ---

    private void OnViewerStartup(Entity<SeeOverLayerComponent> ent, ref ComponentStartup args)
    {
        // Only refresh if the local player just gained this component.
        if (_player.LocalEntity != ent.Owner)
            return;

        RefreshAll(GetLocalPlayerLayers());
    }

    private void OnViewerShutdown(Entity<SeeOverLayerComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        RefreshAll(GetLocalPlayerLayers());
    }

    // --- Player attachment events ---

    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
    {
        RefreshAll(GetLocalPlayerLayers());
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        RefreshAll(new HashSet<string>());
    }

    // --- Helpers ---

    /// <summary>Returns the layer set of the current local player mob, or empty if none.</summary>
    private HashSet<string> GetLocalPlayerLayers()
    {
        if (_player.LocalEntity is not { } local)
            return new HashSet<string>();

        return TryComp<SeeOverLayerComponent>(local, out var comp)
            ? comp.Layers
            : new HashSet<string>();
    }

    /// <summary>Reapplies draw depth overrides for every obstacle entity in the world.</summary>
    private void RefreshAll(HashSet<string> playerLayers)
    {
        var query = EntityQueryEnumerator<SeeOverLayerVisualsComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var visuals, out var sprite))
        {
            UpdateDrawDepth((uid, sprite), visuals, playerLayers);
        }
    }

    private void UpdateDrawDepth(
        Entity<SpriteComponent?> ent,
        SeeOverLayerVisualsComponent visuals,
        HashSet<string> playerLayers)
    {
        var canSeeOver = playerLayers.Contains(visuals.Layer);
        var depth = canSeeOver ? visuals.SeeOverDrawDepth : visuals.NormalDrawDepth;
        _sprite.SetDrawDepth(ent, depth);
    }
}
