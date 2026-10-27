using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.VoiceChat.UI;

public sealed partial class VoiceChatStatusCard : BoxContainer, IDocumentTag
{
    [Dependency] private VoiceChatManager _voice = default!;

    private static readonly Color ConnectedColor = Color.FromHex("#5CD65C");

    private readonly Label _status;
    private readonly Button _connect;

    public VoiceChatStatusCard()
    {
        IoCManager.InjectDependencies(this);

        Orientation = LayoutOrientation.Horizontal;
        SeparationOverride = 6;

        _status = new Label
        {
            HorizontalExpand = true,
            VAlign = Label.VAlignMode.Center,
        };
        _connect = new Button
        {
            Text = Loc.GetString("news-voice-connect"),
            StyleClasses = { HullStyleClasses.Plate },
        };
        _connect.OnPressed += _ => _voice.OpenLink();

        AddChild(_status);
        AddChild(_connect);
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        control = this;
        return true;
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();

        _voice.LinkChanged += UpdateState;
        _voice.WebConnectedChanged += OnWebConnectedChanged;
        _voice.RequestLink();
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
        _status.Text = Loc.GetString(_voice.WebConnected ? "voice-link-connected" : "voice-link-disconnected");
        _status.FontColorOverride = _voice.WebConnected ? ConnectedColor : null;
        _connect.Visible = !_voice.WebConnected;
        _connect.Disabled = _voice.LinkUrl == null;
    }
}
