// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.IconSmoothing;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic;

public sealed class RustRuneSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RustRuneComponent, ComponentStartup>(OnStartup, after: new[] { typeof(IconSmoothSystem) });
        SubscribeLocalEvent<RustRuneComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RustRuneComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
        SubscribeLocalEvent<RustRuneComponent, IconSmoothCornersInitializedEvent>(OnIconSmoothInit);

        SubscribeLocalEvent<SpriteRandomOffsetComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(Entity<SpriteRandomOffsetComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null || !args.AppearanceData.TryGetValue(OffsetVisuals.Offset, out var offset))
            return;

        args.Sprite.Offset = (Vector2) offset;
    }

    private void OnIconSmoothInit(Entity<RustRuneComponent> ent, ref IconSmoothCornersInitializedEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveLayers(sprite);
        AddLayers(uid, comp, sprite);
    }

    private void OnAfterAutoHandleState(Entity<RustRuneComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        AddLayers(uid, comp, sprite);
    }

    private void OnShutdown(Entity<RustRuneComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveLayers(sprite);
    }

    private void OnStartup(Entity<RustRuneComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        AddLayers(uid, comp, sprite);
    }

    private void RemoveLayers(SpriteComponent sprite)
    {
        if (sprite.LayerMapTryGet(RustRuneKey.Rune, out var rune))
            sprite.RemoveLayer(rune);

        if (sprite.LayerMapTryGet(RustRuneKey.Overlay, out var overlay))
            sprite.RemoveLayer(overlay);
    }

    private void AddLayers(EntityUid uid, RustRuneComponent comp, SpriteComponent sprite)
    {
        var diagonal = _tag.HasTag(uid, comp.DiagonalTag);

        if (comp.RustOverlay && !sprite.LayerMapTryGet(RustRuneKey.Overlay, out _))
        {
            var layer = sprite.AddLayer(diagonal ? comp.DiagonalSprite : comp.OverlaySprite);
            sprite.LayerMapSet(RustRuneKey.Overlay, layer);
        }

        if (comp.RuneIndex >= 0 && comp.RuneIndex < comp.RuneSprites.Count)
        {
            if (!sprite.LayerMapTryGet(RustRuneKey.Rune, out var layer))
            {
                layer = sprite.AddLayer(comp.RuneSprites[comp.RuneIndex]);
                sprite.LayerMapSet(RustRuneKey.Rune, layer);
                sprite.LayerSetShader(RustRuneKey.Rune, "unshaded");
            }

            if (comp.AnimationEnded)
            {
                sprite.LayerSetTexture(layer,
                    _spriteSystem.RsiStateLike(comp.RuneSprites[comp.RuneIndex])
                        .GetFrame(RsiDirection.South, comp.LastFrame));
            }

            var offset = diagonal ? comp.DiagonalOffset : comp.RuneOffset;
            sprite.LayerSetOffset(layer, offset);
        }
    }
}