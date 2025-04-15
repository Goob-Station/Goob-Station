// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using System;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Server.GameTicking.Events;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Database;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Serilog;

namespace Content.Goobstation.Server.MisandryBox.GrassEnforce;

public interface IGrassEnforcementManager
{
    public void Initialize();
    public void Shutdown();
    public Task RoundEnd();
}

public sealed class GrassEnforcementManager : IGrassEnforcementManager, IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playtime = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IBanManager _ban = default!;
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private ISawmill _saw = default!;

    private bool _enabled = true;
    private int _resetDayOffset = 30;
    private int _banDayDuration = 14;
    private int _hours = 200;
    private bool _affectAdmins = true;

    // Add a dictionary to track ongoing operations per user
    private readonly Dictionary<NetUserId, Task> _ongoingChecks = new();

    public void Initialize()
    {
        _playtime.AddedMinutes += PlayTimeUpdate;
        _saw = Logger.GetSawmill("GrassEnforce");
    }

    public void Shutdown()
    {
        _playtime.AddedMinutes -= PlayTimeUpdate;
    }

    void IPostInjectInit.PostInject()
    {
        _cfg.OnValueChanged(GoobCVars.GrassEnforce, value => _enabled = value, true);
        _cfg.OnValueChanged(GoobCVars.GrassDayReset, value => _resetDayOffset = value, true);
        _cfg.OnValueChanged(GoobCVars.GrassBanDays, value => _banDayDuration = value, true);
        _cfg.OnValueChanged(GoobCVars.GrassThreshold, value => _hours = value, true);
        _cfg.OnValueChanged(GoobCVars.GrassAffectAdmins, value => _affectAdmins = value, true);
    }

    private async Task PlayTimeUpdate(ICommonSession player, int minutes)
    {
        if (!_enabled)
            return;

        await _db.ModifyPlayedGrassMinutes(player.UserId, minutes);

        // Ensure we don't have multiple concurrent checks for the same user
        if (_ongoingChecks.TryGetValue(player.UserId, out var existingTask))
        {
            // Wait for existing task to complete before starting a new one
            await existingTask;
        }

        var checkTask = CheckClient(player);
        _ongoingChecks[player.UserId] = checkTask;

        try
        {
            await checkTask;
        }
        finally
        {
            // Remove the task from tracking once completed
            _ongoingChecks.Remove(player.UserId);
        }
    }

    private async Task CheckClient(ICommonSession player)
    {
        var reset = await _db.GetGrassResetAfter(player.UserId);
        var minutes = await _db.GetPlayedGrassMinutes(player.UserId);

        _saw.Debug($"Checking {player.Name}. Reset is {reset}, minutes: {minutes}");
        _saw.Debug($"Banning at {_hours*60}");

        if (reset < DateTime.Now)
        {
            await Reset(player.UserId);
            return;
        }

        if (minutes >= _hours * 60)
        {
            await Grass(player);
        }
    }

    private async Task Reset(NetUserId player)
    {
        await _db.ResetPlayedGrassMinutes(player);
        await _db.ResetGrassResetAfter(player, TimeSpan.FromDays(_resetDayOffset));
    }

    private async Task Grass(ICommonSession player)
    {
        if (_admin.IsAdmin(player, true) && !_affectAdmins)
        {
            _saw.Debug($"Bounced - Attempted to grass an admin ({player.Name})!");

            await Reset(player.UserId);
            return;
        }

        _ban.CreateServerBan(player.UserId,
            null,
            null,
            null, // Evading only proves that a wellness check is in order
            null, // Evading only proves that a wellness check is in order
            (uint)_banDayDuration*24*60,
            NoteSeverity.None,
            _cfg.GetCVar(GoobCVars.GrassBanReason));

        await Reset(player.UserId);
    }

    public async Task RoundEnd()
    {
        if (!_enabled)
            return;

        var tasks = new List<Task>();

        foreach (var player in _player.Sessions)
        {
            if (_ongoingChecks.ContainsKey(player.UserId))
                continue;

            var checkTask = CheckClient(player);
            _ongoingChecks[player.UserId] = checkTask;

            tasks.Add(checkTask.ContinueWith(_ => _ongoingChecks.Remove(player.UserId)));
        }

        // Wait for all checks to complete
        await Task.WhenAll(tasks);
    }
}
