using System.Diagnostics.CodeAnalysis;
using Content.Client._Funkystation.Radio;
using Content.Client.Resources;
using Content.Shared.Silicons.StationAi;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.Silicons.StationAi;

public sealed partial class SpeakerTag : IMarkupTagHandler
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IEntitySystemManager _entitySystem = default!;

    public string Name => "speaker";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        string? name = null;
        if (node.Value.TryGetString(out var val) && !string.IsNullOrEmpty(val))
            name = val;
        else if (node.Attributes.TryGetValue("name", out var nameParam) && nameParam.TryGetString(out var nameStr))
            name = nameStr;
        else if (node.Attributes.TryGetValue("text", out var textParam) && textParam.TryGetString(out var textStr))
            name = textStr;

        if (string.IsNullOrEmpty(name))
        {
            control = null;
            return false;
        }

        NetEntity? speaker = null;
        if (node.Attributes.TryGetValue("target", out var targetParam))
        {
            if (targetParam.TryGetLong(out var targetLong))
                speaker = new NetEntity((int) targetLong.Value);
            else if (targetParam.TryGetString(out var targetStr) && NetEntity.TryParse(targetStr, out var parsedTarget))
                speaker = parsedTarget;
        }

        if (speaker == null && node.Attributes.TryGetValue("speaker", out var speakerParam))
        {
            if (speakerParam.TryGetLong(out var speakerLong))
                speaker = new NetEntity((int) speakerLong.Value);
            else if (speakerParam.TryGetString(out var speakerStr) && NetEntity.TryParse(speakerStr, out var parsedSpeaker))
                speaker = parsedSpeaker;
        }

        var label = new Label
        {
            Text = name,
            MouseFilter = Control.MouseFilterMode.Stop,
        };

        label.FontOverride = _cache.GetFont("/Fonts/NotoSans/NotoSans-Bold.ttf", 12);

        Color? textColor = null;
        if (node.Attributes.TryGetValue("channel", out var chanParam) && chanParam.TryGetString(out var chanId))
        {
            if (_entitySystem.TryGetEntitySystem<RadioChannelColorSystem>(out var colorSys))
                textColor = colorSys.GetColor(chanId);
        }

        if (textColor == null && node.Attributes.TryGetValue("color", out var colorParam) && colorParam.TryGetString(out var colorHex))
        {
            if (Color.TryFromHex(colorHex, out var parsedColor))
                textColor = parsedColor;
        }

        if (textColor != null)
            label.FontColorOverride = textColor.Value;

        if (speaker != null)
        {
            label.OnMouseEntered += _ =>
            {
                if (_playerManager.LocalEntity is { } local && _entMan.HasComponent<StationAiHeldComponent>(local))
                {
                    label.DefaultCursorShape = Control.CursorShape.Hand;
                    label.FontColorOverride = textColor != null ? textColor.Value.WithAlpha(0.75f) : Color.LightBlue;
                    label.ToolTip = Loc.GetString("station-ai-teleport-to-speaker-tooltip", ("name", name));
                }
                else
                {
                    label.DefaultCursorShape = Control.CursorShape.Arrow;
                    label.ToolTip = null;
                }
            };

            label.OnMouseExited += _ =>
            {
                label.DefaultCursorShape = Control.CursorShape.Arrow;
                label.FontColorOverride = textColor;
            };

            label.OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                if (_playerManager.LocalEntity is not { } local || !_entMan.HasComponent<StationAiHeldComponent>(local))
                    return;

                _entMan.EntityNetManager.SendSystemNetworkMessage(new StationAiTeleportToTargetEvent(speaker.Value));
                args.Handle();
            };
        }

        control = label;
        return true;
    }
}
