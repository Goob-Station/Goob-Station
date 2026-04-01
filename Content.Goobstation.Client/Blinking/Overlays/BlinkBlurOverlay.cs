// SPDX-FileCopyrightText: 2026 Site-14 Contributors
//
// SPDX-License-Identifier: MPL-2.0
//
// Additional Use Restrictions apply:
// See /LICENSES/SITE14-ADDENDUM.md

using Content.Goobstation.Shared.Blinking;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Maths;

namespace Content.Goobstation.Client.Blinking.Overlays;

public sealed class BlinkBlurOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    private static readonly ProtoId<ShaderPrototype> Shader = "BlinkBlur";
    private readonly ShaderInstance _blurShader;

    private float _blurIntensity;

    public BlinkBlurOverlay()
    {
        IoCManager.InjectDependencies(this);
        _blurShader = _prototypeManager.Index(Shader).InstanceUnique();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_entityManager.TryGetComponent(_playerManager.LocalEntity, out EyeComponent? eyeComp))
            return false;

        if (args.Viewport.Eye != eyeComp.Eye)
            return false;

        var playerEntity = _playerManager.LocalEntity;
        if (playerEntity == null)
            return false;

        if (!_entityManager.TryGetComponent<BlinkingComponent>(playerEntity, out var blinkComp))
            return false;

        var blinkingSys = _sysMan.GetEntitySystem<BlinkingSystem>();
        _blurIntensity = blinkingSys.GetBlinkUrgency(blinkComp);

        return _blurIntensity > 0.01f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var handle = args.WorldHandle;
        _blurShader.SetParameter("SCREEN_TEXTURE", ScreenTexture);
        _blurShader.SetParameter("BlurIntensity", _blurIntensity);
        handle.UseShader(_blurShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
