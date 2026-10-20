using System.Diagnostics.CodeAnalysis;
using Content.Shared.Ghost;
using Content.Shared.Radio;
using Content.Shared.Silicons.StationAi;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed partial class SpeakerTag : IMarkupTagHandler
{
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    public string Name => "speaker";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        string? name = null;
        if (node.Attributes.TryGetValue("name", out var nameParam) && nameParam.StringValue != null)
            name = nameParam.StringValue;
        else if (node.Value.StringValue != null)
            name = node.Value.StringValue;

        if (string.IsNullOrEmpty(name))
            return false;

        name = name.Trim(' ', '"', '=');

        NetEntity? speaker = null;
        if (node.Attributes.TryGetValue("speaker", out var speakerParam))
        {
            if (speakerParam.TryGetLong(out var speakerLong))
            {
                speaker = new NetEntity((int) speakerLong.Value);
            }
            else if (speakerParam.TryGetString(out var speakerStr))
            {
                var trimmed = speakerStr.Trim();
                if (trimmed.StartsWith("n", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("e", StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = trimmed[1..];
                }

                if (NetEntity.TryParse(trimmed, out var parsedSpeaker))
                    speaker = parsedSpeaker;
                else if (int.TryParse(trimmed, out var parsedInt))
                    speaker = new NetEntity(parsedInt);
            }
        }

        Color? color = null;
        if (node.Attributes.TryGetValue("color", out var colorParam)
            && colorParam.StringValue != null
            && Color.TryFromHex(colorParam.StringValue, out var parsedColor))
        {
            color = parsedColor;
        }
        else if (node.Attributes.TryGetValue("channel", out var channelParam)
            && channelParam.StringValue != null
            && _prototype.TryIndex<RadioChannelPrototype>(channelParam.StringValue, out var channelProto))
        {
            color = channelProto.Color;
        }

        var label = new Label
        {
            Text = name,
            MouseFilter = Control.MouseFilterMode.Stop,
        };

        if (color != null)
            label.FontColorOverride = color.Value;

        var font = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Bold.ttf"), 12);
        label.FontOverride = font;

        if (speaker != null)
        {
            var speakerEnt = speaker.Value;
            var speakerName = name;

            label.OnMouseEntered += _ =>
            {
                if (_playerManager.LocalEntity is { } local && CanWarp(local))
                {
                    label.DefaultCursorShape = Control.CursorShape.Hand;
                    label.ToolTip = Loc.GetString("station-ai-teleport-to-speaker-tooltip", ("name", speakerName));
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
                label.ToolTip = null;
            };

            label.OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                if (_playerManager.LocalEntity is not { } local)
                    return;

                if (!CanWarp(local))
                    return;

                _entMan.EntityNetManager.SendSystemNetworkMessage(new StationAiTeleportToTargetEvent(speakerEnt));
                args.Handle();
            };
        }

        control = label;
        return true;
    }

    private bool CanWarp(EntityUid uid)
    {
        return _entMan.HasComponent<StationAiHeldComponent>(uid) ||
               _entMan.HasComponent<StationAiCoreComponent>(uid) ||
               _entMan.HasComponent<StationAiEyeComponent>(uid) ||
               _entMan.HasComponent<GhostComponent>(uid);
    }
}
