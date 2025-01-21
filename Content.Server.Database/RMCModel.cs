﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Content.Server.Database;

[Table("rmc_discord_accounts")]
public sealed class RMCDiscordAccount
{
    [Key]
    public ulong Id { get; set; }

    public RMCLinkedAccount LinkedAccount { get; set; } = default!;
    public List<RMCLinkedAccountLogs> LinkedAccountLogs { get; set; } = default!;
}

[Table("rmc_linked_accounts")]
public sealed class RMCLinkedAccount
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public ulong DiscordId { get; set; }

    public RMCDiscordAccount Discord { get; set; } = default!;
}

[Table("rmc_patron_tiers")]
public sealed class RMCPatronTier
{
    [Key]
    public int Id { get; set; }

    public bool ShowOnCredits { get; set; }

    public bool GhostColor { get; set; }

    public bool LobbyMessage { get; set; }

    public bool RoundEndShoutout { get; set; }

    public string Name { get; set; } = default!;

    public ulong DiscordRole { get; set; }

    public int Priority { get; set; }

    public List<RMCPatron> Patrons { get; set; } = default!;
}

[Table("rmc_patrons")]
[Index(nameof(TierId))]
public sealed class RMCPatron
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public int TierId { get; set; }

    public RMCPatronTier Tier { get; set; } = default!;
    public int? GhostColor { get; set; } = default!;
    public RMCPatronLobbyMessage? LobbyMessage { get; set; } = default!;
    public RMCPatronRoundEndNTShoutout? RoundEndNTShoutout { get; set; } = default!;
}

[Table("rmc_linking_codes")]
[Index(nameof(Code))]
public sealed class RMCLinkingCodes
{
    [Key]
    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public Guid Code { get; set; }

    public DateTime CreationTime { get; set; }
}

[Table("rmc_linked_accounts_logs")]
[Index(nameof(PlayerId))]
[Index(nameof(DiscordId))]
[Index(nameof(At))]
public sealed class RMCLinkedAccountLogs
{
    [Key]
    public int Id { get; set; }

    public Guid PlayerId { get; set; }

    public Player Player { get; set; } = default!;

    public ulong DiscordId { get; set; }

    public RMCDiscordAccount Discord { get; set; } = default!;

    public DateTime At { get; set; }
}

[Table(("rmc_patron_lobby_messages"))]
public sealed class RMCPatronLobbyMessage
{
    [Key, ForeignKey("Patron")]
    public Guid PatronId { get; set; }

    public RMCPatron Patron { get; set; } = default!;

    [StringLength(500)]
    public string Message { get; set; } = default!;
}

[Table(("rmc_patron_round_end_nt_shoutouts"))]
public sealed class RMCPatronRoundEndNTShoutout
{
    [Key, ForeignKey("Patron")]
    public Guid PatronId { get; set; }

    public RMCPatron Patron { get; set; } = default!;

    [StringLength(100), Required]
    public string Name { get; set; } = default!;
}
