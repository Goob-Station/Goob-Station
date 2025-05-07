// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Hologram;
using Content.Shared.Holopad;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Goobstation.Client.Hologram;

public sealed class HologramSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HologramComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<HologramComponent, BeforePostShaderRenderEvent>(OnShaderRender);
    }

    private void OnComponentStartup(Entity<HologramComponent> entity, ref ComponentStartup args)
    {
        UpdateHologramAppearance(entity);
    }

    private void OnShaderRender(Entity<HologramComponent> entity, ref BeforePostShaderRenderEvent ev)
    {
        if (ev.Sprite.PostShader == null)
            return;

        UpdateHologramAppearance(entity);
    }

    private void UpdateHologramAppearance(Entity<HologramComponent> entity)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        // Apply basic sprite settings
        sprite.Color = Color.White;

        // Remove shading from all layers (except displacement maps)
        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            if (sprite.TryGetLayer(i, out var layer) && layer.ShaderPrototype != "DisplacedStencilDraw")
                sprite.LayerSetShader(i, "unshaded");
        }

        // Apply the shader
        UpdateHologramShader(entity);
    }

    private void UpdateHologramShader(EntityUid uid, SpriteComponent? sprite = null, HologramComponent? hologram = null)
    {
        if(!Resolve(uid, ref sprite, ref hologram) || string.IsNullOrWhiteSpace(hologram.ShaderName))
            return;

        float texHeight = sprite.AllLayers.Max(x => x.PixelSize.Y);
        var instance = _prototypeManager.Index<ShaderPrototype>(hologram.ShaderName).InstanceUnique();

        // Set shader parameters
        instance.SetParameter("color1", new Vector3(hologram.Color1.R, hologram.Color1.G, hologram.Color1.B));
        instance.SetParameter("color2", new Vector3(hologram.Color2.R, hologram.Color2.G, hologram.Color2.B));
        instance.SetParameter("alpha", hologram.Alpha);
        instance.SetParameter("intensity", hologram.Intensity);
        instance.SetParameter("texHeight", texHeight);
        instance.SetParameter("t", (float) _timing.CurTime.TotalSeconds * hologram.ScrollRate);

        sprite.PostShader = instance;
        sprite.RaiseShaderEvent = true;
    }
}
