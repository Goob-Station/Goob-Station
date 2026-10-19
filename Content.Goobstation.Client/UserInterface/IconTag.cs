using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Patrons;
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

namespace Content.Goobstation.Client.UserInterface;

public sealed partial class IconTag : IMarkupTag
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
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

        SpriteSpecifier? sprite = null;
        if (_prototype.TryIndex<OocIconPrototype>(id.StringValue, out var oocIcon))
            sprite = oocIcon.Icon;
        else if (_prototype.TryIndex<JobIconPrototype>(id.StringValue, out var iconPrototype))
            sprite = iconPrototype.Icon;

        NetEntity? speaker = null;
        if (node.Attributes.TryGetValue("speaker", out var speakerParam))
        {
            if (speakerParam.TryGetLong(out var speakerLong))
                speaker = new NetEntity((int) speakerLong.Value);
            else if (speakerParam.TryGetString(out var speakerStr) && NetEntity.TryParse(speakerStr, out var parsedSpeaker))
                speaker = parsedSpeaker;
        }

        var icon = new AnimatedTextureRect
        {
            SetWidth = 20,
            SetHeight = 20,
            MouseFilter = Control.MouseFilterMode.Stop,
        };
        icon.DisplayRect.Stretch = TextureRect.StretchMode.Scale;

        if (sprite != null)
            icon.SetFromSpriteSpecifier(sprite);

        if (speaker != null)
        {
            icon.OnMouseEntered += _ =>
            {
                if (_playerManager.LocalEntity is { } local && _entMan.HasComponent<StationAiHeldComponent>(local))
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

                if (_playerManager.LocalEntity is not { } local || !_entMan.HasComponent<StationAiHeldComponent>(local))
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
