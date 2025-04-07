// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Client.Interactable.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client.Stealth;

public sealed class StealthSystem : SharedStealthSystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private ShaderInstance _shader = default!;

    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index<ShaderPrototype>("Stealth").InstanceUnique();

        SubscribeLocalEvent<StealthComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<StealthComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StealthComponent, BeforePostShaderRenderEvent>(OnShaderRender);
    }

    public override void SetEnabled(EntityUid uid, bool value, StealthComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.Enabled == value)
            return;

        base.SetEnabled(uid, value, component);
        SetShader(uid, value, component);
    }

    private void SetShader(EntityUid uid, bool enabled, StealthComponent? component = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref component, ref sprite, false))
            return;

        sprite.Color = Color.White;
        sprite.PostShader = enabled ? _shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;

        if (!enabled)
        {
            if (component.HadOutline && !TerminatingOrDeleted(uid))
                EnsureComp<InteractionOutlineComponent>(uid);
            return;
        }

        if (TryComp(uid, out InteractionOutlineComponent? outline))
        {
            RemCompDeferred(uid, outline);
            component.HadOutline = true;
        }
    }

    private void OnStartup(EntityUid uid, StealthComponent component, ComponentStartup args)
    {
        SetShader(uid, component.Enabled, component);
    }

    private void OnShutdown(EntityUid uid, StealthComponent component, ComponentShutdown args)
    {
        if (!Terminating(uid))
            SetShader(uid, false, component);
    }

    private void OnShaderRender(EntityUid uid, StealthComponent component, BeforePostShaderRenderEvent args)
    {
        // Distortion effect uses screen coordinates. If a player moves, the entities appear to move on screen. this
        // makes the distortion very noticeable.

        // So we need to use relative screen coordinates. The reference frame we use is the parent's position on screen.
        // this ensures that if the Stealth is not moving relative to the parent, its relative screen position remains
        // unchanged.
        var parent = Transform(uid).ParentUid;
        if (!parent.IsValid())
            return; // should never happen, but lets not kill the client.
        var parentXform = Transform(parent);
        var reference = args.Viewport.WorldToLocal(_transformSystem.GetWorldPosition(parentXform));
        reference.X = -reference.X;
        var visibility = GetVisibility(uid, component);

        // Goobstation - Proper invisibility: changes -1 to -1.5
        // actual visual visibility effect is limited to -1.5 to 1.
        visibility = Math.Clamp(visibility, -1.5f, 1f);

        _shader.SetParameter("reference", reference);
        _shader.SetParameter("visibility", visibility);

        visibility = MathF.Max(0, visibility);
        args.Sprite.Color = new Color(visibility, visibility, 1, 1);
    }
}
