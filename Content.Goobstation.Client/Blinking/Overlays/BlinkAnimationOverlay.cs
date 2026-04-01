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

public sealed class BlinkAnimationOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private static readonly ProtoId<ShaderPrototype> Shader = "BlinkEyelid";
    private readonly ShaderInstance _eyelidShader;

    private float _eyelidClosure;

    public BlinkAnimationOverlay()
    {
        IoCManager.InjectDependencies(this);
        _eyelidShader = _prototypeManager.Index(Shader).InstanceUnique();
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

        if (!blinkComp.IsBlinking && !blinkComp.IsHoldingClosed)
            return false;

        var blinkingSys = _sysMan.GetEntitySystem<BlinkingSystem>();
        _eyelidClosure = blinkingSys.GetEyelidClosure(blinkComp);

        return _eyelidClosure > 0.01f;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var handle = args.WorldHandle;

        _eyelidShader.SetParameter("EyelidClosure", _eyelidClosure);
        handle.UseShader(_eyelidShader);
        handle.DrawRect(args.WorldBounds, Color.White);
        handle.UseShader(null);
    }
}
