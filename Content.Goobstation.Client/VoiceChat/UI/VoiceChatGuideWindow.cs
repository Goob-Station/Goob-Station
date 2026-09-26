using System.Numerics;
using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.Input;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class VoiceChatGuideWindow : FancyWindow
{
    [Dependency] private readonly IInputManager _input = default!;

    private static readonly string[] Sections = { "talking", "radio", "station", "controls", "rules" };

    public event Action? NeverAskPressed;

    public VoiceChatGuideWindow(bool prompt)
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("voice-guide-title");
        SetSize = new Vector2(640, 660);
        MinSize = new Vector2(560, 440);

        var guide = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(0, 0, 10, 0),
            SeparationOverride = 4,
        };
        guide.AddChild(CreateText(Loc.GetString("voice-guide-intro")));

        var keys = new (string, object)[]
        {
            ("local", KeyName(ContentKeyFunctions.VoicePushToTalk)),
            ("radio", KeyName(ContentKeyFunctions.VoicePushToTalkRadio)),
        };

        foreach (var section in Sections)
        {
            guide.AddChild(new Label
            {
                Text = Loc.GetString($"voice-guide-{section}-title"),
                StyleClasses = { StyleClass.LabelHeading },
                Margin = new Thickness(0, 8, 0, 0),
            });
            guide.AddChild(CreateText(Loc.GetString($"voice-guide-{section}", keys)));
        }

        var scroll = new ScrollContainer
        {
            HScrollEnabled = false,
            VerticalExpand = true,
        };
        scroll.AddChild(guide);

        var close = new Button
        {
            Text = Loc.GetString(prompt ? "voice-prompt-later" : "voice-link-close"),
            StyleClasses = { prompt ? StyleClass.ButtonOpenRight : StyleClass.ButtonSquare },
        };
        close.OnPressed += _ => Close();

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0),
        };
        buttons.AddChild(close);

        if (prompt)
        {
            var never = new Button
            {
                Text = Loc.GetString("voice-prompt-never"),
                StyleClasses = { StyleClass.ButtonOpenLeft },
            };
            never.OnPressed += _ =>
            {
                NeverAskPressed?.Invoke();
                Close();
            };
            buttons.AddChild(never);
        }

        var body = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8),
        };
        body.AddChild(scroll);
        body.AddChild(new PanelContainer
        {
            StyleClasses = { StyleClass.LowDivider },
            Margin = new Thickness(0, 8, 0, 8),
        });
        body.AddChild(new Label
        {
            Text = Loc.GetString("voice-guide-connect-title"),
            StyleClasses = { StyleClass.LabelHeadingBigger },
        });
        body.AddChild(new VoiceChatConnectPanel());
        body.AddChild(buttons);

        ContentsContainer.AddChild(body);
    }

    private static RichTextLabel CreateText(string markup)
    {
        var label = new RichTextLabel { HorizontalExpand = true };
        label.SetMessage(FormattedMessage.FromMarkupPermissive(markup));
        return label;
    }

    private string KeyName(BoundKeyFunction function)
    {
        return _input.TryGetKeyBinding(function, out var binding)
            ? FormattedMessage.EscapeText(binding.GetKeyString())
            : Loc.GetString("voice-guide-unbound");
    }
}
