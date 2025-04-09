// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Construction;
using Content.Shared.Construction.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Construction;

/// <inheritdoc/>
public sealed class FlatpackSystem : SharedFlatpackSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlatpackComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<FlatpackComponent> ent, ref AppearanceChangeEvent args)
    {
        var (_, comp) = ent;
        if (!_appearance.TryGetData<string>(ent, FlatpackVisuals.Machine, out var machineBoardId) || args.Sprite == null)
            return;

        if (!PrototypeManager.TryIndex<EntityPrototype>(machineBoardId, out var machineBoardPrototype))
            return;

        if (!machineBoardPrototype.TryGetComponent<SpriteComponent>(out var sprite, EntityManager.ComponentFactory))
            return;

        Color? color = null;
        foreach (var layer in sprite.AllLayers)
        {
            if (layer.RsiState.Name is not { } spriteState)
                continue;

            if (!comp.BoardColors.TryGetValue(spriteState, out var c))
                continue;
            color = c;
            break;
        }

        if (color != null)
            args.Sprite.LayerSetColor(FlatpackVisualLayers.Overlay, color.Value);
    }
}