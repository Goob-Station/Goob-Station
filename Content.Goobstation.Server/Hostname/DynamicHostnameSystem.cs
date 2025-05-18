using Content.Server.GameTicking;
using Robust.Shared;
using Robust.Shared.Configuration;
using Content.Goobstation.Common.CCVar;
using Robust.Shared.Prototypes;
using Content.Shared.Dataset;
using Robust.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Goobstation.Common.JoinQueue;
using Robust.Shared.Timing;
using System;

namespace Content.Server.DynamicHostname;


/// <summary>
/// This handles dynamically updating hostnames.
/// </summary>
public sealed class DynamicHostnameSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IJoinQueueManager _queue = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static ProtoId<LocalizedDatasetPrototype> _messagesProto = "MessageOfTheDay";
    private LocalizedDatasetPrototype? _messages;
    private string _originalHostname = string.Empty;
    private TimeSpan _nextUpdateTime;
    private static TimeSpan _updateInterval = TimeSpan.FromSeconds(10);
    private bool _dynHostEnabled = false;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_configuration, GoobCVars.UseDynamicHostname, OnDynHostChange, true);
        Subs.CVar(_configuration, CVars.HubAdvertiseInterval, OnHubAdIntChange, true);
        _originalHostname = _configuration.GetCVar(CVars.GameHostName);
        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
        _messages = _proto.Index(_messagesProto);
    }

    private void OnHubAdIntChange(int newValue)
        => _updateInterval = TimeSpan.FromSeconds(newValue);

    private void OnDynHostChange(bool newValue)
        => _dynHostEnabled = newValue;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_gameTiming.CurTime >= _nextUpdateTime && _dynHostEnabled)
        {
            UpdateHostname();
            _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
        }
    }

    private void UpdateHostname()
    {
        var hostname = _originalHostname;

        if (_messages != null)
            hostname += " | " + _random.Pick(_messages);

        if (_queue.PlayerInQueueCount > 0)
            hostname += " | Queue: " + _queue.PlayerInQueueCount + " players";

        _configuration.SetCVar(CVars.GameHostName, hostname);
    }
}
