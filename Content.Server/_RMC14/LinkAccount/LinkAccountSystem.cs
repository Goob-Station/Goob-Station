// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
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
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Shared._RMC14.GhostColor;
using Content.Shared._RMC14.LinkAccount;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._RMC14.LinkAccount;

public sealed class LinkAccountSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly LinkAccountManager _linkAccount = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _timeBetweenLobbyMessages;
    private TimeSpan _nextLobbyMessageTime;
    private TimeSpan _lobbyMessageInitialDelay;
    private (string Message, string User)? _nextLobbyMessage;
    private string? _nextNTShoutout;

    public override void Initialize()
    {
        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnGameRunLevelChanged);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);

        SubscribeLocalEvent<GhostColorComponent, PlayerAttachedEvent>(OnGhostColorPlayerAttached);

        Subs.CVar(_config, GoobCVars.RMCPatronLobbyMessageTimeSeconds, v => _timeBetweenLobbyMessages = TimeSpan.FromSeconds(v), true);
        Subs.CVar(_config, GoobCVars.RMCPatronLobbyMessageInitialDelaySeconds, v => _lobbyMessageInitialDelay = TimeSpan.FromSeconds(v), true);

        ReloadPatrons();
        GetRandomLobbyMessage();
        GetRandomShoutout();

        _linkAccount.PatronUpdated += OnPatronUpdated;
    }

    public override void Shutdown()
    {
        _linkAccount.PatronUpdated -= OnPatronUpdated;
    }

    private void OnGameRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        switch (ev.New)
        {
            case GameRunLevel.InRound:
                GetRandomShoutout();
                break;
            case GameRunLevel.PreRoundLobby:
            case GameRunLevel.PostRound:
                ReloadPatrons();
                GetRandomLobbyMessage();
                GetRandomShoutout();
                break;
        }

        if (ev.New == GameRunLevel.PreRoundLobby)
            _nextLobbyMessageTime = _timing.RealTime + _lobbyMessageInitialDelay;
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        if (_nextNTShoutout != null)
        {
            ev.AddLine("\n");
            ev.AddLine(Loc.GetString("rmc-ui-shoutout-nt", ("name", _nextNTShoutout)));
        }
    }

    private void OnGhostColorPlayerAttached(Entity<GhostColorComponent> ent, ref PlayerAttachedEvent args)
    {
        if (!TryComp(ent, out ActorComponent? actor) ||
            _linkAccount.GetPatron(actor.PlayerSession.UserId) is not { } patron ||
            patron.Tier is not { GhostColor: true } ||
            patron.GhostColor is not { } color)
        {
            RemCompDeferred<GhostColorComponent>(ent);
            return;
        }

        ent.Comp.Color = color;
        Dirty(ent);
    }

    private async void ReloadPatrons()
    {
        try
        {
            await _linkAccount.RefreshAllPatrons();
            _linkAccount.SendPatronsToAll();
        }
        catch (Exception e)
        {
            Log.Error($"Error reloading Patrons list:\n{e}");
        }
    }

    private async void GetRandomLobbyMessage()
    {
        try
        {
            _nextLobbyMessage = await _db.GetRandomLobbyMessage();
        }
        catch (Exception e)
        {
            Log.Error($"Error getting random lobby message:\n{e}");
        }
    }

    private async void GetRandomShoutout()
    {
        try
        {
            (_nextNTShoutout) = await _db.GetRandomShoutout();
        }
        catch (Exception e)
        {
            Log.Error($"Error getting random shoutout:\n{e}");
        }
    }

    private void OnPatronUpdated((NetUserId Id, SharedRMCPatronFull Patron) tuple)
    {
        if (_player.TryGetSessionById(tuple.Id, out var session) &&
            session.AttachedEntity is { } ent &&
            HasComp<GhostComponent>(ent))
        {
            var color = EnsureComp<GhostColorComponent>(ent);
            color.Color = tuple.Patron.GhostColor;
            Dirty(ent, color);
        }
    }

    public override void Update(float frameTime)
    {
        var time = _timing.RealTime;
        if (time < _nextLobbyMessageTime)
            return;

        _nextLobbyMessageTime = time + _timeBetweenLobbyMessages;

        if (_nextLobbyMessage is { } message)
            RaiseNetworkEvent(new SharedRMCDisplayLobbyMessageEvent(message.Message, message.User));

        GetRandomLobbyMessage();
    }
}