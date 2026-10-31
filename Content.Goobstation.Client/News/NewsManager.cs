using System.Globalization;
using System.Linq;
using Content.Client.Guidebook;
using Content.Goobstation.Client.VoiceChat;
using Content.Goobstation.Common.CCVar;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.News;

public sealed partial class NewsManager
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IDependencyCollection _deps = default!;
    [Dependency] private DocumentParsingManager _parsing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IResourceManager _res = default!;
    [Dependency] private VoiceChatManager _voice = default!;

    public const int FeedLimit = 10;
    private const string NeverShow = "never";

    private readonly List<NewsEntryPrototype> _pinned = new();
    private readonly List<NewsEntryPrototype> _posts = new();
    private readonly HashSet<string> _read = new();
    private readonly Dictionary<string, string> _popupState = new();
    private readonly HashSet<string> _warnedMissing = new();
    private ISawmill _sawmill = default!;

    public event Action? Changed;

    public IReadOnlyList<NewsEntryPrototype> Pinned => _pinned;
    public IReadOnlyList<NewsEntryPrototype> Posts => _posts;
    public int UnreadCount { get; private set; }

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("news");

        LoadState();

        _cfg.OnValueChanged(GoobCVars.NewsPinned, _ => Refresh());
        _cfg.OnValueChanged(GoobCVars.VoiceChatEnabled, _ => Refresh());
        _voice.WebConnectedChanged += _ => Refresh();
        _proto.PrototypesReloaded += OnPrototypesReloaded;

        Refresh();
    }

    public static string FormatDate(DateTime date)
    {
        return date.ToString("d MMM yyyy", CultureInfo.InvariantCulture);
    }

    public bool IsVisible(NewsEntryPrototype entry)
    {
        foreach (var condition in entry.Conditions)
        {
            if (!condition.Check(_deps))
                return false;
        }

        return true;
    }

    public bool IsUnread(NewsEntryPrototype entry)
    {
        return entry.Date != null && !_pinned.Contains(entry) && !_read.Contains(entry.ID);
    }

    public void MarkRead(NewsEntryPrototype entry)
    {
        if (!_read.Add(entry.ID))
            return;

        _cfg.SetCVar(GoobCVars.NewsRead, string.Join(',', _read));
        _cfg.SaveToFile();
        UpdateUnread();
        Changed?.Invoke();
    }

    public List<NewsEntryPrototype> GetDuePopups()
    {
        var due = new List<NewsEntryPrototype>();
        var now = DateTime.UtcNow;

        foreach (var entry in _proto.EnumeratePrototypes<NewsEntryPrototype>().OrderBy(e => PinIndex(e)).ThenBy(e => e.ID))
        {
            if (entry.Popup is not { } popup || !IsVisible(entry))
                continue;

            if (popup.Conditions.Any(c => !c.Check(_deps)))
                continue;

            _popupState.TryGetValue(entry.ID, out var state);
            if (state == NeverShow)
                continue;

            var lastShown = DateTime.TryParse(state, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
                ? parsed
                : (DateTime?) null;

            var isDue = popup.Frequency switch
            {
                NewsPopupFrequency.Once => lastShown == null,
                NewsPopupFrequency.EveryConnect => true,
                NewsPopupFrequency.Days => lastShown == null || (now - lastShown.Value).TotalDays >= popup.Days,
                _ => false,
            };

            if (isDue)
                due.Add(entry);
        }

        return due;
    }

    public void RecordPopupShown(NewsEntryPrototype entry)
    {
        if (_popupState.TryGetValue(entry.ID, out var state) && state == NeverShow)
            return;

        _popupState[entry.ID] = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        SavePopupState();
    }

    public void SetNeverShow(NewsEntryPrototype entry)
    {
        _popupState[entry.ID] = NeverShow;
        SavePopupState();
    }

    public bool TryRender(Control target, ResPath path)
    {
        if (!_res.ContentFileExists(path))
        {
            _sawmill.Error($"News document {path} doesn't exist");
            target.AddChild(new Label { Text = Loc.GetString("news-document-error") });
            return false;
        }

        return _parsing.TryAddMarkup(target, _res.ContentFileReadAllText(path));
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<NewsEntryPrototype>())
            Refresh();
    }

    private void Refresh()
    {
        _pinned.Clear();
        _posts.Clear();

        foreach (var id in SplitIds(_cfg.GetCVar(GoobCVars.NewsPinned)))
        {
            if (!_proto.TryIndex<NewsEntryPrototype>(id, out var entry))
            {
                if (_warnedMissing.Add(id))
                    _sawmill.Warning($"news.pinned lists {id}, but there is no newsEntry with that ID");
                continue;
            }

            if (!_pinned.Contains(entry) && IsVisible(entry))
                _pinned.Add(entry);
        }

        foreach (var entry in _proto.EnumeratePrototypes<NewsEntryPrototype>())
        {
            if (entry.Date != null && !_pinned.Contains(entry) && IsVisible(entry))
                _posts.Add(entry);
        }

        _posts.Sort((a, b) =>
        {
            var byDate = b.Date!.Value.CompareTo(a.Date!.Value);
            return byDate != 0 ? byDate : string.CompareOrdinal(a.ID, b.ID);
        });

        if (_posts.Count > FeedLimit)
            _posts.RemoveRange(FeedLimit, _posts.Count - FeedLimit);

        UpdateUnread();
        Changed?.Invoke();
    }

    private void UpdateUnread()
    {
        UnreadCount = _posts.Count(IsUnread);
    }

    private int PinIndex(NewsEntryPrototype entry)
    {
        var index = _pinned.IndexOf(entry);
        return index < 0 ? int.MaxValue : index;
    }

    private void LoadState()
    {
        foreach (var id in SplitIds(_cfg.GetCVar(GoobCVars.NewsRead)))
        {
            _read.Add(id);
        }

        foreach (var pair in _cfg.GetCVar(GoobCVars.NewsPopupState).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var split = pair.IndexOf('=');
            if (split <= 0)
                continue;

            _popupState[pair[..split].Trim()] = pair[(split + 1)..].Trim();
        }
    }

    private void SavePopupState()
    {
        _cfg.SetCVar(GoobCVars.NewsPopupState, string.Join(';', _popupState.Select(p => $"{p.Key}={p.Value}")));
        _cfg.SaveToFile();
    }

    private static IEnumerable<string> SplitIds(string value)
    {
        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
