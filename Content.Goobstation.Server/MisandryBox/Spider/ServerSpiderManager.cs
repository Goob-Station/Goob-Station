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

    private List<Guid> _friends = [];

    public void PostInject()
    {
        _net.RegisterNetMessage<SpiderConsentMsg>(RequestSpider);
        _net.RegisterNetMessage<SpiderClearMsg>();
        _net.RegisterNetMessage<SpiderMsg>();

        _net.Connected += OnConnected;
    }

    private void OnConnected(object? sender, NetChannelArgs e)
    {
        if (_friends.Contains(e.Channel.UserId.UserId))
            AddPermanentSpider(e.Channel);
    }

    private async void RequestSpider(SpiderConsentMsg message)
    {
        // USER HAS REQUESTED A SPIDER, WE HAVE TO GIVE HIM A SPIDER!
        await _db.AddPermanentSpiderFriend(message.MsgChannel.UserId);

        AddPermanentSpider(message.MsgChannel);
    }

    public void Initialize()
    {
        Task.Run(async () => _friends = await _db.GetAllSpiderUserIds());
    }

    public void RequestSpider()
    {
        // None, client method
    }

    public void AddTemporarySpider(ICommonSession? victim = null)
    {
        if (victim is null)
            return;

        _net.ServerSendMessage(new SpiderMsg { Permanent = false }, victim.Channel);
    }

    public void AddPermanentSpider(INetChannel channel)
    {
        var sess = _player.GetSessionByChannel(channel);
        AddPermanentSpider(sess);
    }

    public void AddPermanentSpider(ICommonSession? victim = null)
    {
        if (victim is null)
            return;

        _net.ServerSendMessage(new SpiderMsg { Permanent = true }, victim.Channel);
    }

    public void ClearTemporarySpiders()
    {
        // Client method, as usual
    }
}
