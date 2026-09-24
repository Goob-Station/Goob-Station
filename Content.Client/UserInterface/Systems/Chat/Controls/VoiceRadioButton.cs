using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.Radio;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class VoiceRadioButton : Button
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public readonly VoiceRadioPopup Popup;

    private ProtoId<RadioChannelPrototype>? _shownActive;
    private bool _initialized;
    private uint _frameLastPopupChanged;

    public VoiceRadioButton()
    {
        IoCManager.InjectDependencies(this);
        Name = "VoiceRadioButton";
        MinWidth = 60;
        Visible = false;
        ToggleMode = true;
        ToolTip = Loc.GetString("voice-radio-button-tooltip");
        OnToggled += OnButtonToggled;

        Popup = UserInterfaceManager.CreatePopup<VoiceRadioPopup>();
        Popup.OnVisibilityChanged += OnPopupVisibilityChanged;
        Popup.TalkSelected += OnTalkSelected;
    }

    public void UpdateState()
    {
        if (!_cfg.GetCVar(GoobCVars.VoiceChatEnabled) ||
            !_cfg.GetCVar(GoobCVars.VoiceChatRadioEnabled) ||
            _player.LocalEntity is not { } local ||
            !_entManager.TryGetComponent<VoiceRadioComponent>(local, out var radio) ||
            radio.Channels.Count == 0 && radio.Receive.Count == 0)
        {
            Visible = false;
            if (Popup.Visible)
                Popup.Close();
            return;
        }

        Visible = true;
        Popup.SetState(radio);

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

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        if (_frameLastPopupChanged == _timing.CurFrame)
            return;

        base.KeyBindDown(args);
    }

    private void OnButtonToggled(ButtonToggledEventArgs args)
    {
        if (!args.Pressed)
        {
            Popup.Close();
            return;
        }

        var content = Popup.MeasureContent();
        var size = new Vector2(MathF.Max(content.X, Width), content.Y);
        var screen = UserInterfaceManager.PopupRoot.Size;

        var x = Math.Clamp(GlobalPosition.X, 0f, MathF.Max(0f, screen.X - size.X));
        var below = GlobalPosition.Y + Height;
        var y = below + size.Y <= screen.Y
            ? below
            : MathF.Max(0f, GlobalPosition.Y - size.Y);

        Popup.Open(UIBox2.FromDimensions(new Vector2(x, y), new Vector2(Width, 0f)));
    }

    private void OnPopupVisibilityChanged(Control control)
    {
        Pressed = control.Visible;
        _frameLastPopupChanged = _timing.CurFrame;
    }

    private void OnTalkSelected(ProtoId<RadioChannelPrototype>? channel)
    {
        Popup.Close();
        _net.SendSystemNetworkMessage(new VoiceRadioSelectEvent(channel));
    }
}

public sealed class VoiceRadioPopup : Popup
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const float LabelWidth = 48f;
    private const int TalkColumns = 5;
    private const int HearColumns = 4;

    private readonly PanelContainer _panel;
    private readonly BoxContainer _talkRow;
    private readonly GridContainer _talkItems;
    private readonly BoxContainer _hearRow;
    private readonly GridContainer _hearItems;
    private readonly Slider _volume;
    private readonly Label _volumeValue;
    private readonly List<ProtoId<RadioChannelPrototype>> _talkChannels = new();
    private readonly List<ProtoId<RadioChannelPrototype>> _hearChannels = new();
    private readonly Dictionary<ProtoId<RadioChannelPrototype>, Button> _talkButtons = new();
    private readonly Dictionary<ProtoId<RadioChannelPrototype>, CheckBox> _hearBoxes = new();

    private Button? _localButton;
    private ProtoId<RadioChannelPrototype>? _active;
    private string? _mutedValue;
    private HashSet<string> _muted = new();
    private bool _settingsDirty;
    private bool _built;

    public event Action<ProtoId<RadioChannelPrototype>?>? TalkSelected;

    public VoiceRadioPopup()
    {
        IoCManager.InjectDependencies(this);

        _talkItems = new GridContainer { Columns = TalkColumns, HSeparationOverride = 2, VSeparationOverride = 2 };
        _talkRow = CreateRow("voice-radio-popup-talk", _talkItems);

        _hearItems = new GridContainer { Columns = HearColumns, HSeparationOverride = 8, VSeparationOverride = 2 };
        _hearRow = CreateRow("voice-radio-popup-hear", _hearItems);

        _volume = new Slider
        {
            MinValue = 0f,
            MaxValue = 200f,
            Rounded = true,
            MinWidth = 140f,
            HorizontalExpand = true,
            VerticalAlignment = VAlignment.Center,
        };
        _volume.OnValueChanged += _ => OnVolumeChanged();
        _volumeValue = new Label { MinWidth = 40f, Align = Label.AlignMode.Right };

        var volumeItems = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6, HorizontalExpand = true };
        volumeItems.AddChild(_volume);
        volumeItems.AddChild(_volumeValue);
        var volumeRow = CreateRow("voice-radio-popup-volume", volumeItems);

        var body = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 6,
            Margin = new Thickness(8, 6),
        };
        body.AddChild(_talkRow);
        body.AddChild(_hearRow);
        body.AddChild(volumeRow);

        _panel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromHex("#1B1B1E").WithAlpha(0.95f),
                BorderColor = Color.FromHex("#3B3E56"),
                BorderThickness = new Thickness(1),
            },
        };
        _panel.AddChild(body);
        AddChild(_panel);

        OnPopupOpen += SyncVolume;
        OnPopupHide += SaveSettings;
    }

    public Vector2 MeasureContent()
    {
        _panel.Measure(Vector2Helpers.Infinity);
        return _panel.DesiredSize;
    }

    public void SetState(VoiceRadioComponent radio)
    {
        var mutedValue = _cfg.GetCVar(GoobCVars.VoiceChatRadioMuted);
        var mutedChanged = mutedValue != _mutedValue;
        if (mutedChanged)
        {
            _mutedValue = mutedValue;
            _muted = ParseMuted(mutedValue);
        }

        if (!_built || !_talkChannels.SequenceEqual(radio.Channels))
        {
            RebuildTalk(radio.Channels);
            _active = null;
            _built = true;
            UpdateTalkHighlight(radio.Active);
        }
        else if (_active != radio.Active)
        {
            UpdateTalkHighlight(radio.Active);
        }

        if (!_hearChannels.SequenceEqual(radio.Receive))
        {
            RebuildHear(radio.Receive);
            mutedChanged = true;
        }

        if (mutedChanged)
        {
            foreach (var (id, box) in _hearBoxes)
            {
                box.Pressed = !_muted.Contains(id.Id);
            }
        }
    }

    private BoxContainer CreateRow(string label, Control items)
    {
        var row = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6 };
        row.AddChild(new Label
        {
            Text = Loc.GetString(label),
            MinWidth = LabelWidth,
            FontColorOverride = Color.FromHex("#A9A9B3"),
            VerticalAlignment = VAlignment.Top,
        });
        row.AddChild(items);
        return row;
    }

    private void RebuildTalk(List<ProtoId<RadioChannelPrototype>> channels)
    {
        _talkChannels.Clear();
        _talkChannels.AddRange(channels);
        _talkButtons.Clear();
        _talkItems.RemoveAllChildren();
        _talkRow.Visible = channels.Count > 0;

        _localButton = CreateTalkButton(Loc.GetString("voice-radio-button-off"), null);
        foreach (var id in channels)
        {
            if (_prototype.TryIndex(id, out var channel))
                _talkButtons[id] = CreateTalkButton(channel.LocalizedName, id);
        }
    }

    private Button CreateTalkButton(string text, ProtoId<RadioChannelPrototype>? channel)
    {
        var button = new Button
        {
            Text = text,
            StyleClasses = { ChannelSelectorItemButton.StyleClassChatSelectorOptionButton },
        };
        button.OnPressed += _ => TalkSelected?.Invoke(channel);
        _talkItems.AddChild(button);
        return button;
    }

    private void UpdateTalkHighlight(ProtoId<RadioChannelPrototype>? active)
    {
        _active = active;

        if (_localButton != null)
            _localButton.Modulate = active == null ? Color.White : Color.DarkGray.WithAlpha(0.6f);

        foreach (var (id, button) in _talkButtons)
        {
            var color = _prototype.TryIndex(id, out var channel) ? channel.Color : Color.White;
            button.Modulate = id == active ? color : color.WithAlpha(0.45f);
        }
    }

    private void RebuildHear(List<ProtoId<RadioChannelPrototype>> channels)
    {
        _hearChannels.Clear();
        _hearChannels.AddRange(channels);
        _hearBoxes.Clear();
        _hearItems.RemoveAllChildren();
        _hearRow.Visible = channels.Count > 0;

        foreach (var id in channels)
        {
            if (!_prototype.TryIndex(id, out var channel))
                continue;

            var box = new CheckBox { Text = channel.LocalizedName };
            box.Label.FontColorOverride = channel.Color;
            box.OnToggled += args => SetHearing(id, args.Pressed);
            _hearBoxes[id] = box;
            _hearItems.AddChild(box);
        }
    }

    private void SetHearing(ProtoId<RadioChannelPrototype> channel, bool hearing)
    {
        var changed = hearing ? _muted.Remove(channel.Id) : _muted.Add(channel.Id);
        if (!changed)
            return;

        var ordered = _muted.ToList();
        ordered.Sort(string.CompareOrdinal);
        _mutedValue = string.Join(',', ordered);
        _cfg.SetCVar(GoobCVars.VoiceChatRadioMuted, _mutedValue);
        _settingsDirty = true;
    }

    private void SyncVolume()
    {
        var percent = MathF.Round(Math.Clamp(_cfg.GetCVar(GoobCVars.VoiceChatRadioVolume), 0f, 2f) * 100f);
        _volume.SetValueWithoutEvent(percent);
        _volumeValue.Text = Loc.GetString("voice-radio-popup-volume-value", ("percent", (int) percent));
    }

    private void OnVolumeChanged()
    {
        var percent = (int) MathF.Round(_volume.Value);
        _volumeValue.Text = Loc.GetString("voice-radio-popup-volume-value", ("percent", percent));
        _cfg.SetCVar(GoobCVars.VoiceChatRadioVolume, percent / 100f);
        _settingsDirty = true;
    }

    private void SaveSettings()
    {
        if (!_settingsDirty)
            return;

        _settingsDirty = false;
        _cfg.SaveToFile();
    }

    private static HashSet<string> ParseMuted(string value)
    {
        var channels = new HashSet<string>();
        foreach (var part in value.Split(','))
        {
            var channel = part.Trim();
            if (channel.Length > 0)
                channels.Add(channel);
        }

        return channels;
    }
}
