using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.Radio;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class VoiceRadioButton : ChatPopupButton<VoiceRadioPopup>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const int DropdownOffset = 38;

    private ProtoId<RadioChannelPrototype>? _shownActive;
    private bool _initialized;

    public VoiceRadioButton()
    {
        IoCManager.InjectDependencies(this);
        Name = "VoiceRadioButton";
        MinWidth = 60;
        Visible = false;
        ToolTip = Loc.GetString("voice-radio-button-tooltip");
        Popup.Selected += OnSelected;
    }

    protected override UIBox2 GetPopupPosition()
    {
        return UIBox2.FromDimensions(
            new Vector2(GlobalPosition.X, GlobalPosition.Y + Height),
            new Vector2(SizeBox.Width, DropdownOffset));
    }

    public void UpdateState()
    {
        if (!_cfg.GetCVar(GoobCVars.VoiceChatEnabled) ||
            !_cfg.GetCVar(GoobCVars.VoiceChatRadioEnabled) ||
            _player.LocalEntity is not { } local ||
            !_entManager.TryGetComponent<VoiceRadioComponent>(local, out var radio) ||
            radio.Channels.Count == 0)
        {
            Visible = false;
            if (Popup.Visible)
                Popup.Close();
            return;
        }

        Visible = true;
        Popup.SetChannels(radio.Channels);

        if (_initialized && _shownActive == radio.Active)
            return;

        _initialized = true;
        _shownActive = radio.Active;

        if (radio.Active is { } active && _prototype.TryIndex(active, out var channel))
        {
            Text = channel.LocalizedName;
            Modulate = channel.Color;
        }
        else
        {
            Text = Loc.GetString("voice-radio-button-off");
            Modulate = Color.DarkGray;
        }
    }

    private void OnSelected(ProtoId<RadioChannelPrototype>? channel)
    {
        Popup.Close();
        _net.SendSystemNetworkMessage(new VoiceRadioSelectEvent(channel));
    }
}

public sealed class VoiceRadioPopup : Popup
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly BoxContainer _items;
    private readonly List<ProtoId<RadioChannelPrototype>> _channels = new();

    public event Action<ProtoId<RadioChannelPrototype>?>? Selected;

    public VoiceRadioPopup()
    {
        IoCManager.InjectDependencies(this);
        _items = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            SeparationOverride = 1,
        };
        AddChild(_items);
    }

    public void SetChannels(List<ProtoId<RadioChannelPrototype>> channels)
    {
        if (_channels.SequenceEqual(channels))
            return;

        _channels.Clear();
        _channels.AddRange(channels);
        _items.RemoveAllChildren();

        AddItem(Loc.GetString("voice-radio-button-off"), null, Color.DarkGray);
        foreach (var id in channels)
        {
            if (_prototype.TryIndex(id, out var channel))
                AddItem(channel.LocalizedName, id, channel.Color);
        }
    }

    private void AddItem(string text, ProtoId<RadioChannelPrototype>? channel, Color color)
    {
        var item = new Button
        {
            Text = text,
            Modulate = color,
            StyleClasses = { ChannelSelectorItemButton.StyleClassChatSelectorOptionButton },
        };
        item.OnPressed += _ => Selected?.Invoke(channel);
        _items.AddChild(item);
    }
}
