using Content.Client.Lobby;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Shared.MisandryBox.Spider;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.MisandryBox.Spider;

public sealed class ClientSpiderManager : ISpiderManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IStateManager _state = default!;

    private SpiderUIController _spider = default!;
    private bool _permanent = false;

    void IPostInjectInit.PostInject()
    {
        _netMan.RegisterNetMessage<SpiderConsentMsg>();
        _netMan.RegisterNetMessage<SpiderClearMsg>(ClearSpider);
        _netMan.RegisterNetMessage<SpiderMsg>(ProcessSpider);

        _state.OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged(StateChangedEventArgs args)
    {
        if (args.NewState is LobbyState)
            ClearTemporarySpiders();
    }

    public void Initialize()
    {
        _spider = _ui.GetUIController<SpiderUIController>();
    }

    private void ClearSpider(SpiderClearMsg message)
    {
        ClearTemporarySpiders();
    }

    private void ProcessSpider(SpiderMsg message)
    {
        if (message.Permanent)
            AddPermanentSpider();
        else
            AddTemporarySpider();
    }

    public void RequestSpider()
    {
        _netMan.ClientSendMessage(new SpiderConsentMsg());
    }

    public void AddTemporarySpider(ICommonSession? victim = null)
    {
        _spider.SetEnabled(true);
    }

    public void AddPermanentSpider(ICommonSession? victim = null)
    {
        _permanent = true;
        _spider.Permanent = _permanent;
        _spider.SetEnabled(true);
    }

    public void ClearTemporarySpiders()
    {
        if (_permanent)
            return;

        _spider.SetEnabled(false);
    }
}
