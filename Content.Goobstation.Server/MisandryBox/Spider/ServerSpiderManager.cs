// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Shared.MisandryBox.Spider;
using Content.Server.Database;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.MisandryBox.Spider;

public sealed class ServerSpiderManager : ISpiderManager, IPostInjectInit, IEntityEventSubscriber
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    private readonly HashSet<Guid> _friends = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized = false;

    public void PostInject()
    {
        _net.RegisterNetMessage<SpiderConsentMsg>(RequestSpiderAsync);
        _net.RegisterNetMessage<SpiderClearMsg>();
        _net.RegisterNetMessage<SpiderMsg>();

        _net.Connected += OnConnectedAsync;
    }

    private async void OnConnectedAsync(object? sender, NetChannelArgs e)
    {
        await EnsureInitializedAsync();

        lock (_friends)
        {
            if (_friends.Contains(e.Channel.UserId.UserId))
                AddPermanentSpider(e.Channel);
        }
    }

    private async void RequestSpiderAsync(SpiderConsentMsg message)
    {
        await EnsureInitializedAsync();

        lock (_friends)
        {
            if (_friends.Contains(message.MsgChannel.UserId.UserId))
                return;
        }

        try
        {
            await _db.AddPermanentSpiderFriend(message.MsgChannel.UserId);
            AddPermanentSpider(message.MsgChannel);
        }
        catch (Exception ex)
        {
            IoCManager.Resolve<ILogManager>()
                .GetSawmill("Spider")
                .Error($"{message.MsgChannel.UserName} attempted to consent to spider but database operation failed: {ex}");
        }
    }

    public async void Initialize()
    {
        await EnsureInitializedAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            var friends = await _db.GetAllSpiderUserIds();
            lock (_friends)
            {
                _friends.Clear();
                foreach (var friend in friends)
                {
                    _friends.Add(friend);
                }
            }
            _initialized = true;
        }
        catch (Exception ex)
        {
            IoCManager.Resolve<ILogManager>()
                .GetSawmill("Spider")
                .Error($"Failed to initialize spider friends: {ex}");
        }
        finally
        {
            _initLock.Release();
        }
    }

    public void RequestSpider()
    {
        // Client method
    }

    public void AddTemporarySpider(ICommonSession? victim = null)
    {
        if (victim is null)
            return;

        _net.ServerSendMessage(new SpiderMsg { Permanent = false }, victim.Channel);
    }

    private void AddPermanentSpider(INetChannel channel)
    {
        var sess = _player.GetSessionByChannel(channel);
        AddPermanentSpider(sess);
    }

    public void AddPermanentSpider(ICommonSession? victim = null)
    {
        if (victim is null)
            return;

        _net.ServerSendMessage(new SpiderMsg { Permanent = true }, victim.Channel);

        lock (_friends)
        {
            _friends.Add(victim.UserId.UserId);
        }
    }

    public void ClearTemporarySpiders()
    {
        // Client method
    }
}
