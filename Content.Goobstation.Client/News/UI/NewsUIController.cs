using Content.Client.Gameplay;
using Content.Client.Lobby;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.News.UI;

public sealed partial class NewsUIController : UIController, IOnStateEntered<LobbyState>, IOnStateEntered<GameplayState>
{
    [Dependency] private IClientNetManager _net = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private NewsManager _news = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private static readonly TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    private readonly Queue<NewsEntryPrototype> _popupQueue = new();
    private NewsReaderWindow? _reader;
    private NewsReaderWindow? _popup;
    private TimeSpan? _popupAt;
    private bool _checkedPopups;

    public override void Initialize()
    {
        base.Initialize();

        _net.Connected += OnConnected;
    }

    public void OnStateEntered(LobbyState state)
    {
        SchedulePopups();
    }

    public void OnStateEntered(GameplayState state)
    {
        SchedulePopups();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_popupAt is not { } popupAt || _timing.RealTime < popupAt)
            return;

        _popupAt = null;
        if (_checkedPopups || !_net.IsConnected)
            return;

        _checkedPopups = true;
        foreach (var entry in _news.GetDuePopups())
        {
            _popupQueue.Enqueue(entry);
        }

        ShowNextPopup();
    }

    public bool Open(ProtoId<NewsEntryPrototype> id)
    {
        if (!_proto.TryIndex(id, out var entry) || !_news.IsVisible(entry))
            return false;

        if (_reader == null)
        {
            _reader = new NewsReaderWindow();
            _reader.OnClose += () => _reader = null;
        }

        _reader.SetEntry(entry, false);
        if (_reader.IsOpen)
            _reader.MoveToFront();
        else
            _reader.OpenCentered();

        _news.MarkRead(entry);
        return true;
    }

    private void SchedulePopups()
    {
        if (!_checkedPopups && _popupAt == null)
            _popupAt = _timing.RealTime + PopupDelay;
    }

    private void ShowNextPopup()
    {
        if (_popup != null || !_popupQueue.TryDequeue(out var entry))
            return;

        var window = new NewsReaderWindow();
        var never = false;
        window.NeverPressed += () => never = true;
        window.OnClose += () =>
        {
            if (never)
                _news.SetNeverShow(entry);
            else
                _news.RecordPopupShown(entry);

            if (_popup == window)
                _popup = null;

            ShowNextPopup();
        };

        _popup = window;
        window.SetEntry(entry, true);
        window.OpenCentered();
        _news.MarkRead(entry);
    }

    private void OnConnected(object? sender, NetChannelArgs args)
    {
        _checkedPopups = false;
        _popupAt = null;
        _popupQueue.Clear();
        _reader?.Close();
        _popup?.Close();
    }
}
