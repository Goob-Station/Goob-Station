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
using Content.Goobstation.Common.UserInterface;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Tag;
using Robust.Client.GameObjects;
using Robust.Shared.Graphics.RSI;

namespace Content.Goobstation.Client.Heretic;

public sealed class RustRuneSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

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

        // shut up john obsolete
        args.Sprite.Offset = (Vector2) offset;
    }

    private void OnIconSmoothInit(Entity<RustRuneComponent> ent, ref IconSmoothCornersInitializedEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveLayers((uid, sprite));
        AddLayers((uid, sprite), comp);
    }

    private void OnAfterAutoHandleState(Entity<RustRuneComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        AddLayers((uid, sprite), comp);
    }

    private void OnShutdown(Entity<RustRuneComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveLayers((uid, sprite));
    }

    private void OnStartup(Entity<RustRuneComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        AddLayers((uid, sprite), comp);
    }

    private void RemoveLayers(Entity<SpriteComponent?> ent)
    {
        if (_sprite.LayerMapTryGet(ent, RustRuneKey.Rune, out var rune, false))
            _sprite.RemoveLayer(ent, rune);

        if (_sprite.LayerMapTryGet(ent, RustRuneKey.Overlay, out var overlay, false))
            _sprite.RemoveLayer(ent, overlay);
    }

    private void AddLayers(Entity<SpriteComponent?> ent, RustRuneComponent runeComp)
    {
        var diagonal = _tag.HasTag(ent, runeComp.DiagonalTag);

        if (runeComp.RustOverlay && !_sprite.LayerMapTryGet(ent, RustRuneKey.Overlay, out _, false))
        {
            var layer = _sprite.AddLayer(ent, diagonal ? runeComp.DiagonalSprite : runeComp.OverlaySprite);
            _sprite.LayerMapSet(ent, RustRuneKey.Overlay, layer);
        }

        if (runeComp.RuneIndex >= 0 && runeComp.RuneIndex < runeComp.RuneSprites.Count)
        {
            if (!_sprite.LayerMapTryGet(ent, RustRuneKey.Rune, out var layer, false))
            {
                layer = _sprite.AddLayer(ent, runeComp.RuneSprites[runeComp.RuneIndex]);
                _sprite.LayerMapSet(ent, RustRuneKey.Rune, layer);
                ent.Comp!.LayerSetShader(RustRuneKey.Rune, "unshaded");
            }

            if (runeComp.AnimationEnded)
            {
                var state = _sprite.RsiStateLike(runeComp.RuneSprites[runeComp.RuneIndex]);
                var frame = state.GetFrame(RsiDirection.South, runeComp.LastFrame);
                _sprite.LayerSetTexture(ent, layer, frame);
            }

            var offset = diagonal ? runeComp.DiagonalOffset : runeComp.RuneOffset;
            _sprite.LayerSetOffset(ent, layer, offset);
        }
    }
}
