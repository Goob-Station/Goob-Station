// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MPL-2.0

using Content.Client.Lobby;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.MisandryBox;
using Content.Goobstation.Shared.MisandryBox.Spider;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Goobstation.Client.MisandryBox.Spider;

public sealed class ClientSpiderManager : ISpiderManager, IPostInjectInit
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IStateManager _state = default!;
    [Dependency] private readonly IConfigurationManager _conf = default!;

    private SpiderUIController _spider = default!;

    void IPostInjectInit.PostInject()
    {
        _netMan.RegisterNetMessage<SpiderConsentMsg>();
        _netMan.RegisterNetMessage<SpiderClearMsg>(ClearSpider);
        _netMan.RegisterNetMessage<SpiderMsg>(ProcessSpider);

        _netMan.Connected += OnConnected;
        _state.OnStateChanged += OnStateChanged;
    }

    private void OnConnected(object? sender, NetChannelArgs e)
    {
        if (_conf.GetCVar(GoobCVars.SpiderFriend))
            _netMan.ClientSendMessage(new SpiderConsentMsg());
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
        _spider.AddTemporarySpider();
    }

    public void AddPermanentSpider(ICommonSession? victim = null)
    {
        _spider.AddPermanentSpider();

        _conf.SetCVar(GoobCVars.SpiderFriend, true);
        _conf.SaveToFile();
    }

    public void ClearTemporarySpiders()
    {
        _spider.ClearTemporarySpiders();
    }
}
