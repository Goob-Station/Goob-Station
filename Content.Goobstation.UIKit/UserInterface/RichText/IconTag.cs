using System.Diagnostics.CodeAnalysis;
using Content.Shared.Ghost;
using Content.Shared.Silicons.StationAi;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed partial class IconTag : IMarkupTagHandler
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private IEntityManager _entMan = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    private SpriteSystem? _spriteSystem;

    public string Name => "icon";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        return TryGetControl(node, out control);
    }

    public bool TryGetControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Attributes.TryGetValue("src", out var id) || id.StringValue == null)
        {
            control = null;
            return false;
        }
        _spriteSystem ??= _entMan.System<SpriteSystem>();
        var texture = _prototype.TryIndex<JobIconPrototype>(id.StringValue, out var iconPrototype)
                ? _spriteSystem.Frame0(iconPrototype.Icon)
                : null;

        NetEntity? speaker = null;
        if (node.Attributes.TryGetValue("speaker", out var speakerParam))
        {
            if (speakerParam.TryGetLong(out var speakerLong))
                speaker = new NetEntity((int) speakerLong.Value);
            else if (speakerParam.TryGetString(out var speakerStr) && NetEntity.TryParse(speakerStr, out var parsedSpeaker))
                speaker = parsedSpeaker;
        }

        var icon = new TextureRect
        {
            Texture = texture,
            SetWidth = 20,
            SetHeight = 20,
            Stretch = TextureRect.StretchMode.Scale,
            MouseFilter = Control.MouseFilterMode.Stop,
        };

        if (speaker != null)
        {
            icon.OnMouseEntered += _ =>
            {
                if (_playerManager.LocalEntity is { } local && (_entMan.HasComponent<StationAiHeldComponent>(local) || _entMan.HasComponent<GhostComponent>(local)))
                    icon.DefaultCursorShape = Control.CursorShape.Hand;
                else
                    icon.DefaultCursorShape = Control.CursorShape.Arrow;
            };

            icon.OnMouseExited += _ =>
            {
                icon.DefaultCursorShape = Control.CursorShape.Arrow;
            };

            icon.OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;

                if (_playerManager.LocalEntity is not { } local)
                    return;

                if (!_entMan.HasComponent<StationAiHeldComponent>(local) && !_entMan.HasComponent<GhostComponent>(local))
                    return;

                _entMan.EntityNetManager.SendSystemNetworkMessage(new StationAiTeleportToTargetEvent(speaker.Value));
                args.Handle();
            };
        }

        if (node.Attributes.TryGetValue("tooltip", out var tooltip) && tooltip.StringValue != null)
            icon.ToolTip = tooltip.StringValue;
        control = icon;
        return true;
    }
}
