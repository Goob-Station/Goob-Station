using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MartialArts;

public sealed partial class MartialArtsComboOverlay : Overlay
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IInputManager _inputMan = default!;
    [Dependency] private ILogManager _logMan = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IResourceCache _resourceCache = default!;

    private ISawmill _sawmill = default!;
    private EntityQuery<ComboPerformerComponent> _comboQuery = default!;

    private static readonly ResPath ComboAttackRsi =
        new ResPath("/Textures/_Goobstation/Interface/Misc/intents.rsi");

    public MartialArtsComboOverlay()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = _logMan.GetSawmill("ComboOverlay");
        _comboQuery = _entMan.GetEntityQuery<ComboPerformerComponent>();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (!_cfg.GetCVar(GoobCVars.MartialArtsOverlayEnabled))
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_comboQuery.TryComp(_player.LocalEntity, out var comboComp))
            return;

        var attacks = comboComp.LastAttacks;

        if (attacks.Count == 0)
            return;

        var screen = args.ScreenHandle;
        var color = Color.White.WithAlpha(0.75f);
        var mousePos = _inputMan.MouseScreenPosition;

        var offset = _cfg.GetCVar(CCVars.HudHeldItemOffset);
        var offsetVec = new Vector2(offset, offset);

        for (var i = 0; i < attacks.Count; i++)
        {
            var attack = comboComp.LastAttacks[i];
            var rsi = _resourceCache.GetResource<RSIResource>(ComboAttackRsi).RSI;
            var stateId = attack.ToString().ToLower(); // todo slop
            if (!rsi.TryGetState(stateId, out var state))
            {
                _sawmill.Error($"Missing combo sprite state '{stateId}'!");
                continue;
            }

            var texture = state.Frame0;
            var size = texture.Size * 2;
            var offsetVec2 = new Vector2(
                -offsetVec.X - size.Y / 1.8f,
                (2f * i + 1f - attacks.Count) * size.Y / 1.8f);

            screen.DrawTextureRect(texture,
                UIBox2.FromDimensions(mousePos.Position - size / 2 + offsetVec2, size),
                color);
        }
    }
}
