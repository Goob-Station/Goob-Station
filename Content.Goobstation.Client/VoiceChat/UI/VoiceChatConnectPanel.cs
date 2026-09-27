using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed class VoiceChatConnectPanel : BoxContainer
{
    [Dependency] private readonly IClipboardManager _clipboard = default!;
    [Dependency] private readonly VoiceChatManager _voice = default!;

    private static readonly Color ConnectedColor = Color.FromHex("#5CD65C");

    private readonly Label _status;
    private readonly Button _open;
    private readonly Button _copy;
    private readonly Label _pageUrl;
    private readonly Label _code;

    public VoiceChatConnectPanel()
    {
        IoCManager.InjectDependencies(this);

        Orientation = LayoutOrientation.Vertical;
        SeparationOverride = 6;

        _status = new Label();

        _open = new Button
        {
            Text = Loc.GetString("voice-link-open"),
            StyleClasses = { StyleClass.ButtonOpenRight },
            HorizontalExpand = true,
        };
        _copy = new Button
        {
            Text = Loc.GetString("voice-link-copy"),
            StyleClasses = { StyleClass.ButtonOpenLeft },
            HorizontalExpand = true,
        };
        _open.OnPressed += _ => _voice.OpenLink();
        _copy.OnPressed += _ =>
        {
            if (_voice.LinkUrl is { } url)
                _clipboard.SetText(url);
        };

        var buttons = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            VerticalAlignment = VAlignment.Bottom,
            VerticalExpand = true,
        };
        buttons.AddChild(_open);
        buttons.AddChild(_copy);

        var computer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 1f,
            SeparationOverride = 4,
        };
        computer.AddChild(new Label
        {
            Text = Loc.GetString("voice-link-computer-title"),
            StyleClasses = { StyleClass.LabelHeading },
        });
        computer.AddChild(new Label
        {
            Text = Loc.GetString("voice-link-computer-text"),
            StyleClasses = { StyleClass.LabelSubText },
        });
        computer.AddChild(buttons);

        _pageUrl = new Label
        {
            StyleClasses = { StyleClass.LabelKeyText },
        };
        _code = new Label
        {
            StyleClasses = { StyleClass.LabelHeadingBigger },
            HorizontalAlignment = HAlignment.Center,
        };

        var phone = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SizeFlagsStretchRatio = 1f,
            SeparationOverride = 4,
        };
        phone.AddChild(new Label
        {
            Text = Loc.GetString("voice-link-phone-title"),
            StyleClasses = { StyleClass.LabelHeading },
        });
        phone.AddChild(new Label
        {
            Text = Loc.GetString("voice-link-phone-text"),
            StyleClasses = { StyleClass.LabelSubText },
        });
        phone.AddChild(_pageUrl);
        phone.AddChild(_code);

        var columns = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            SeparationOverride = 16,
        };
        columns.AddChild(computer);
        columns.AddChild(phone);

        AddChild(_status);
        AddChild(columns);
        AddChild(new Label
        {
            Text = Loc.GetString("voice-link-expiry"),
            StyleClasses = { StyleClass.LabelSubText },
        });
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();

        _voice.LinkChanged += UpdateState;
        _voice.WebConnectedChanged += OnWebConnectedChanged;
        UpdateState();
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

        _voice.LinkChanged -= UpdateState;
        _voice.WebConnectedChanged -= OnWebConnectedChanged;
    }

    private void OnWebConnectedChanged(bool connected)
    {
        UpdateState();
    }

    private void UpdateState()
    {
        var ready = _voice.LinkUrl != null;

        _status.Text = Loc.GetString(_voice.WebConnected ? "voice-link-connected" : "voice-link-disconnected");
        _status.FontColorOverride = _voice.WebConnected ? ConnectedColor : null;
        _pageUrl.Text = _voice.PageUrl ?? Loc.GetString("voice-link-loading");
        _code.Text = _voice.LinkCode ?? "--------";
        _open.Disabled = !ready;
        _copy.Disabled = !ready;
    }
}
