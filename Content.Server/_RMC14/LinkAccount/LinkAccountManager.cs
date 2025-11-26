// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 MortalBaguette <169563638+MortalBaguette@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Poips <Hanakohashbrown@gmail.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 blobadoodle <me@bloba.dev>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._RMC14.LinkAccount;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Color = System.Drawing.Color;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, TimeSpan> _lastRequest = new();
    private readonly TimeSpan _minimumWait = TimeSpan.FromSeconds(0.5);
    private readonly Dictionary<NetUserId, SharedRMCPatronFull> _connected = new();
    private readonly List<SharedRMCPatron> _allPatrons = [];
    private readonly List<(string Message, string User)> _lobbyMessages = [];
    private readonly List<string> _shoutouts = [];

    public event Action? PatronsReloaded;
    public event Action<(NetUserId Id, SharedRMCPatronFull Patron)>? PatronUpdated;

    private async Task LoadData(ICommonSession player, CancellationToken cancel)
    {
        var patron = await _db.GetPatron(player.UserId, cancel);
        var linked = await _db.HasLinkedAccount(player.UserId, cancel);
        cancel.ThrowIfCancellationRequested();

        var tier = patron?.Tier;
        var sharedTier = tier == null
            ? null
            : new SharedRMCPatronTier(
                tier.ShowOnCredits,
                tier.GhostColor,
                tier.LobbyMessage,
                tier.RoundEndShoutout,
                tier.Name
            );

        SharedRMCLobbyMessage? lobbyMessage = null;
        if (patron?.LobbyMessage is { Message.Length: > 0 } patronMsg)
            lobbyMessage = new SharedRMCLobbyMessage(patronMsg.Message);

        var ntName = patron?.RoundEndNTShoutout?.Name;
        SharedRMCRoundEndShoutouts? shoutouts = null;
        if (ntName != null)
            shoutouts = new SharedRMCRoundEndShoutouts(ntName);


        Robust.Shared.Maths.Color? ghostColor = null;
        if (patron?.GhostColor is { } patronColor)
        {
            var sysColor = Color.FromArgb(patronColor);
            ghostColor = new Robust.Shared.Maths.Color(sysColor.R, sysColor.G, sysColor.B, sysColor.A);
        }

        _connected[player.UserId] = new SharedRMCPatronFull(sharedTier, linked, ghostColor, lobbyMessage, shoutouts);
    }

    private void FinishLoad(ICommonSession player)
    {
        SendPatronStatus(player);
    }

    private void ClientDisconnected(ICommonSession player)
    {
        _connected.Remove(player.UserId);
    }

    private void SendPatronStatus(ICommonSession player)
    {
        var connected = _connected.GetValueOrDefault(player.UserId);
        var msg = new LinkAccountStatusMsg { Patron = connected, };
        _net.ServerSendMessage(msg, player.Channel);
        SendPatrons(player);
    }

    private void OnRequest(LinkAccountRequestMsg message)
    {
        var user = message.MsgChannel.UserId;
        var time = _timing.RealTime;
        if (_lastRequest.TryGetValue(user, out var last) &&
            last + _minimumWait > time)
        {
            return;
        }

        _lastRequest[user] = time;

        var code = Guid.NewGuid();
        _db.SetLinkingCode(user, code);

        var response = new LinkAccountCodeMsg { Code = code };
        _net.ServerSendMessage(response, message.MsgChannel);
    }

    private void OnClearGhostColor(RMCClearGhostColorMsg message)
    {
        SetGhostColor(message.MsgChannel.UserId, null);
    }

    private void OnChangeGhostColor(RMCChangeGhostColorMsg message)
    {
        SetGhostColor(message.MsgChannel.UserId, message.Color);
    }

    private void OnChangeLobbyMessage(RMCChangeLobbyMessageMsg message)
    {
        var text = message.Text;
        if (text == null)
            return;

        var user = message.MsgChannel.UserId;
        if (GetPatron(user)?.Tier is not { LobbyMessage: true })
            return;

        if (text.Length > SharedRMCLobbyMessage.CharacterLimit)
            text = text[..SharedRMCLobbyMessage.CharacterLimit];

        _db.SetLobbyMessage(user, text);
    }

    private void OnChangeNTShoutout(RMCChangeNTShoutoutMsg message)
    {
        var name = message.Name;
        if (name == null)
            return;

        var user = message.MsgChannel.UserId;
        if (GetPatron(user)?.Tier is not { RoundEndShoutout: true })
            return;

        if (name.Length > SharedRMCRoundEndShoutouts.CharacterLimit)
            name = name[..SharedRMCRoundEndShoutouts.CharacterLimit];

        _db.SetNTShoutout(user, name);
    }

    private void SetGhostColor(NetUserId user, Robust.Shared.Maths.Color? color)
    {
        if (GetPatron(user)?.Tier is not { GhostColor: true })
            return;

        Color? sysColor = color == null ? null : Color.FromArgb(color.Value.ToArgb());
        _db.SetGhostColor(user, sysColor);

        if (_connected.TryGetValue(user, out var connected))
        {
            connected = connected with { GhostColor = color };
            _connected[user] = connected;
            PatronUpdated?.Invoke((user, connected));
        }
    }

    public async Task RefreshAllPatrons()
    {
        var patrons = await _db.GetAllPatrons();
        var messages = await _db.GetLobbyMessages();
        var shoutouts = await _db.GetShoutouts();

        _allPatrons.Clear();
        _lobbyMessages.Clear();
        _shoutouts.Clear();

        foreach (var patron in patrons)
        {
            _allPatrons.Add(new SharedRMCPatron(patron.Player.LastSeenUserName, patron.Tier.Name));
        }

        _lobbyMessages.AddRange(messages);
        _shoutouts.AddRange(shoutouts);

        PatronsReloaded?.Invoke();
    }

    public (string Message, string User)? GetRandomLobbyMessage()
    {
        if (_lobbyMessages.Count == 0)
            return null;

        return _random.Pick(_lobbyMessages);
    }

    public string GetRandomShoutout()
    {
        if (_shoutouts.Count == 0)
            return "John Nanotrasen";

        return _random.Pick(_shoutouts);
    }

    public void SendPatronsToAll()
    {
        var msg = new RMCPatronListMsg { Patrons = _allPatrons };
        _net.ServerSendToAll(msg);
    }

    public void SendPatrons(ICommonSession player)
    {
        var msg = new RMCPatronListMsg { Patrons = _allPatrons };
        _net.ServerSendMessage(msg, player.Channel);
    }

    public SharedRMCPatronFull? GetPatron(ICommonSession player)
    {
        return GetPatron(player.UserId);
    }

    public SharedRMCPatronFull? GetPatron(NetUserId userId)
    {
        return _connected.GetValueOrDefault(userId);
    }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<LinkAccountRequestMsg>(OnRequest);
        _net.RegisterNetMessage<LinkAccountCodeMsg>();
        _net.RegisterNetMessage<LinkAccountStatusMsg>();
        _net.RegisterNetMessage<RMCPatronListMsg>();
        _net.RegisterNetMessage<RMCClearGhostColorMsg>(OnClearGhostColor);
        _net.RegisterNetMessage<RMCChangeGhostColorMsg>(OnChangeGhostColor);
        _net.RegisterNetMessage<RMCChangeLobbyMessageMsg>(OnChangeLobbyMessage);
        _net.RegisterNetMessage<RMCChangeNTShoutoutMsg>(OnChangeNTShoutout);
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnFinishLoad(FinishLoad);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);
    }
}
