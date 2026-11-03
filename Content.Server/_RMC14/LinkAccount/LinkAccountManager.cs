// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Common.Patrons; // Goob
using Content.Server.Database;
using Content.Shared._RMC14.LinkAccount;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes; // Goob
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Color = System.Drawing.Color;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountManager : IPostInjectInit
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!; // Goob
    [Dependency] private readonly ILogManager _log = default!; // Goob
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly UserDbDataManager _userDb = default!;

    private readonly Dictionary<NetUserId, TimeSpan> _lastRequest = new();
    private readonly TimeSpan _minimumWait = TimeSpan.FromSeconds(0.5);
    private readonly Dictionary<NetUserId, SharedRMCPatronFull> _connected = new();
    private readonly Dictionary<NetUserId, ProtoId<PatronTierPrototype>> _fauxPatronAssignments = new(); // Goob
    private bool _tiersSynced; // Goob
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

        var sharedTier = patron == null ? null : GetSharedTier(patron.TierId); // Goob - patron tier prototypes

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

        // Goob start - ghost cosmetics
        SharedRMCGhostCosmetics? ghostCosmetics = null;
        if (patron is { } p && (p.GhostParticles != null || p.GhostHat != null || p.GhostMask != null))
            ghostCosmetics = new SharedRMCGhostCosmetics(p.GhostParticles, p.GhostHat, p.GhostMask);
        // Goob end

        _connected[player.UserId] = new SharedRMCPatronFull(sharedTier, linked, ghostColor, ghostCosmetics, patron?.OocIcon, lobbyMessage, shoutouts); // Goob - ghost cosmetics, ooc icon
    }

    private void FinishLoad(ICommonSession player)
    {
        SendPatronStatus(player);
    }

    private void ClientDisconnected(ICommonSession player)
    {
        _connected.Remove(player.UserId);
    }

    public void SendPatronStatus(ICommonSession player) // Goob - public
    {
        var connected = GetPatron(player.UserId); // Goob - include faux tiers
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

    // Goob start

    public void SetGhostCosmetics(NetUserId user, string? particles, string? hat, string? mask)
    {
        if (GetPatron(user)?.Tier is not { } tier ||
            !tier.GhostCosmetics && !tier.GhostParticles)
        {
            return;
        }

        if (!tier.GhostParticles)
            particles = null;

        if (!tier.GhostCosmetics)
        {
            hat = null;
            mask = null;
        }

        var cosmetics = particles == null && hat == null && mask == null
            ? null
            : new SharedRMCGhostCosmetics(particles, hat, mask);

        _db.SetGhostCosmetics(user, particles, hat, mask);

        if (_connected.TryGetValue(user, out var connected))
        {
            connected = connected with { GhostCosmetics = cosmetics };
            _connected[user] = connected;
            PatronUpdated?.Invoke((user, connected));
        }
    }

    public void SetOocIcon(NetUserId user, string? icon)
    {
        if (GetPatronTier(user) is not { } tier)
            return;

        if (icon != null &&
            (!_prototypes.TryIndex<OocIconPrototype>(icon, out var iconProto) || !tier.CanUse(iconProto)))
        {
            return;
        }

        _db.SetOocIcon(user, icon);

        if (_connected.TryGetValue(user, out var connected))
        {
            connected = connected with { OocIcon = icon };
            _connected[user] = connected;
            PatronUpdated?.Invoke((user, connected));
        }
    }

    public string? GetOocIcon(NetUserId user)
    {
        if (GetPatronTier(user) is not { } tier)
            return null;

        if (GetPatron(user)?.OocIcon is { } selected &&
            _prototypes.TryIndex<OocIconPrototype>(selected, out var icon) &&
            tier.CanUse(icon))
        {
            return icon.ID;
        }

        return tier.Icon;
    }

    public PatronTierPrototype? GetPatronTier(NetUserId user)
    {
        if (GetPatron(user)?.Tier is not { } tier)
            return null;

        return _prototypes.TryIndex(tier.Id, out var proto) ? proto : null;
    }

    private SharedRMCPatronTier? GetSharedTier(int databaseId)
    {
        if (_prototypes.GetPatronTierByDatabaseId(databaseId) is not { } proto)
            return null;

        return GetSharedTier(proto);
    }

    private static SharedRMCPatronTier GetSharedTier(PatronTierPrototype proto)
    {
        return new SharedRMCPatronTier(
            proto.ID,
            proto.ShowOnCredits,
            proto.GhostColor,
            proto.GhostCosmetics,
            proto.GhostParticles,
            proto.LobbyMessage,
            proto.RoundEndShoutout,
            proto.OocIconTiers.Count > 0,
            Loc.GetString(proto.Name),
            proto.Icon
        );
    }

    private async Task SyncTiers()
    {
        var tiers = new List<RMCPatronTier>();
        foreach (var proto in _prototypes.EnumeratePrototypes<PatronTierPrototype>())
        {
            tiers.Add(new RMCPatronTier
            {
                Id = proto.DatabaseId,
                Name = Loc.GetString(proto.Name),
                DiscordRole = proto.DiscordRole,
                Priority = proto.Priority,
                Icon = proto.Icon,
                ShowOnCredits = proto.ShowOnCredits,
                GhostColor = proto.GhostColor,
                GhostCosmetics = proto.GhostCosmetics,
                GhostParticles = proto.GhostParticles,
                LobbyMessage = proto.LobbyMessage,
                RoundEndShoutout = proto.RoundEndShoutout,
            });
        }

        _tiersSynced = true;
        try
        {
            await _db.SyncPatronTiers(tiers);
        }
        catch (Exception e)
        {
            _log.GetSawmill("link_account").Error($"Failed to sync patron tier prototypes to the database:\n{e}");
        }
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<PatronTierPrototype>())
            _tiersSynced = false;
    }

    public async Task ReloadPatron(ICommonSession player)
    {
        await LoadData(player, CancellationToken.None);
        SendPatronStatus(player);

        if (_connected.TryGetValue(player.UserId, out var connected))
            PatronUpdated?.Invoke((player.UserId, connected));
    }
    // Goob end

    public async Task RefreshAllPatrons()
    {
        if (!_tiersSynced) // Goob
            await SyncTiers(); // Goob

        var patrons = await _db.GetAllPatrons();
        var messages = await _db.GetLobbyMessages();
        var shoutouts = await _db.GetShoutouts();

        _allPatrons.Clear();
        _lobbyMessages.Clear();
        _shoutouts.Clear();

        // Goob start - patron tier prototypes
        foreach (var patron in patrons)
        {
            if (_prototypes.GetPatronTierByDatabaseId(patron.TierId) is not { ShowOnCredits: true } tier)
                continue;

            _allPatrons.Add(new SharedRMCPatron(patron.Player.LastSeenUserName, tier.ID));
        }

        foreach (var (message, user, tierId) in messages)
        {
            if (_prototypes.GetPatronTierByDatabaseId(tierId) is { LobbyMessage: true })
                _lobbyMessages.Add((message, user));
        }

        foreach (var (name, tierId) in shoutouts)
        {
            if (_prototypes.GetPatronTierByDatabaseId(tierId) is { RoundEndShoutout: true })
                _shoutouts.Add(name);
        }
        // Goob end

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
        if (_fauxPatronAssignments.TryGetValue(userId, out var tierId) &&
            _prototypes.TryIndex(tierId, out var tier))
        {
            // Goob start
            var connected = _connected.GetValueOrDefault(userId);
            return new SharedRMCPatronFull(
                Tier: GetSharedTier(tier),
                Linked: true,
                GhostColor: connected?.GhostColor,
                GhostCosmetics: connected?.GhostCosmetics,
                OocIcon: connected?.OocIcon,
                LobbyMessage: connected?.LobbyMessage,
                RoundEndShoutout: connected?.RoundEndShoutout
            );
            // Goob end
        }

        return _connected.GetValueOrDefault(userId);
    }

    // Goob start
    public void AssignFauxPatron(NetUserId userId, ProtoId<PatronTierPrototype>? tierId)
    {
        if (tierId == null)
            _fauxPatronAssignments.Remove(userId);
        else if (_prototypes.HasIndex(tierId.Value))
            _fauxPatronAssignments[userId] = tierId.Value;
    }

    public IReadOnlyDictionary<NetUserId, ProtoId<PatronTierPrototype>> GetAllFauxPatronAssignments()
    {
        return _fauxPatronAssignments;
    }
    // Goob end

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
        _prototypes.PrototypesReloaded += OnPrototypesReloaded; // Goob
    }
}
