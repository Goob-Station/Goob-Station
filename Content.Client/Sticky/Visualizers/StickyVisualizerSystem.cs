// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Sticky.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Sticky.Visualizers;

public sealed class StickyVisualizerSystem : VisualizerSystem<StickyVisualizerComponent>
{
    private EntityQuery<SpriteComponent> _spriteQuery;

    public override void Initialize()
    {
        base.Initialize();

        _spriteQuery = GetEntityQuery<SpriteComponent>();

        SubscribeLocalEvent<StickyVisualizerComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<StickyVisualizerComponent> ent, ref ComponentInit args)
    {
        if (!_spriteQuery.TryComp(ent, out var sprite))
            return;

        ent.Comp.OriginalDrawDepth = sprite.DrawDepth;
    }

    protected override void OnAppearanceChange(EntityUid uid, StickyVisualizerComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(uid, StickyVisuals.IsStuck, out var isStuck, args.Component))
            return;

        var drawDepth = isStuck ? comp.StuckDrawDepth : comp.OriginalDrawDepth;
        args.Sprite.DrawDepth = drawDepth;
    }
}
