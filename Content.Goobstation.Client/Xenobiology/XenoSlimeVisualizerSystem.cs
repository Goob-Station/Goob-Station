// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Xenobiology;

/// <summary>
/// This handles visual changes in slimes between breeds.
/// </summary>
public sealed class XenoSlimeVisualizerSystem : VisualizerSystem<SlimeComponent>
{

    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void OnAppearanceChange(EntityUid uid, SlimeComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !AppearanceSystem.TryGetData<Color>(uid, XenoSlimeVisuals.Color, out var color, args.Component))
            return;

        foreach (var layer in args.Sprite.AllLayers)
            layer.Color = color.WithAlpha(layer.Color.A);

        if (AppearanceSystem.TryGetData<ProtoId<ShaderPrototype>>(uid, XenoSlimeVisuals.Shader, out var shader, args.Component))
            args.Sprite.PostShader = _proto.Index(shader).InstanceUnique();
    }
}
