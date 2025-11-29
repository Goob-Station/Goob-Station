using Content.Server.GameTicking;
using Content.Shared._CorvaxGoob.CCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Content.Server._CorvaxGoob.Announcer;

public sealed class AnnouncerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private bool _enabled;
    private AnnouncerPrototype? _announcerToday = null;

    private int? _resetCountdown;
    private AnnouncerPrototype? _forcePresetAnnouncer;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configManager, CCCVars.CalendarAnnouncerEnabled, value => _enabled = value, true);

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
    }

    /// <summary>
    /// Sets announcer for specifically rounds count.
    /// </summary>
    public void ForceSetAnnouncer(AnnouncerPrototype announcer, int rounds = 1)
    {
        _resetCountdown = rounds;
        _forcePresetAnnouncer = announcer;

        if (_gameTicker.RunLevel == GameRunLevel.PreRoundLobby)
        {
            _resetCountdown--;
            _announcerToday = announcer;
        }
    }

    /// <summary>
    /// Saves today announcer prototype to the system data.
    /// </summary>
    private void OnGameRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        if (ev.New != GameRunLevel.PreRoundLobby)
            return;

        if (_resetCountdown is not null)
        {
            if (_resetCountdown > 0)
            {
                _announcerToday = _forcePresetAnnouncer;
                _resetCountdown--;
                return;
            }
            else
            {
                _resetCountdown = null;
                _forcePresetAnnouncer = null;
            }
        }

        if (_resetCountdown is null && _enabled)
            _announcerToday = CalculateAvailableAnnouncerToday();
    }
    public AnnouncerPrototype? GetAnnouncerToday() => _announcerToday;

    public bool TryGetAnnouncerToday([NotNullWhen(true)] out AnnouncerPrototype? announcerPrototype)
    {
        announcerPrototype = _announcerToday;
        return announcerPrototype is not null;
    }

    /// <summary>
    /// Generates calendar for announcer and return if today them day.
    /// </summary>
    private AnnouncerPrototype? CalculateAvailableAnnouncerToday()
    {
        var now = DateTime.Now;

        foreach (var announcerPrototype in _prototype.EnumeratePrototypes<AnnouncerPrototype>())
        {
            var announcerId = announcerPrototype.ID;
            var charsSalt = 1;

            foreach (var character in announcerId)
                charsSalt += announcerId.Count(c => c == character);

            int announcerSeed = 1984 / announcerId.Length + charsSalt * 676179; // unique seed for announcer
            var random = new Random(now.Year + now.Month + announcerSeed);

            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

            int maxRandomDays = random.Next(announcerPrototype.MinDaysInMonth, announcerPrototype.MaxDaysInMonth);
            for (int i = 0; announcerPrototype.MinDaysInMonth > i && i < maxRandomDays; i++)
                if (now.Day == random.Next(1, daysInMonth)) // return if today announcer's day 
                    return _random.Prob(announcerPrototype.Chance) ? announcerPrototype : null;
        }

        return null;
    }
}
