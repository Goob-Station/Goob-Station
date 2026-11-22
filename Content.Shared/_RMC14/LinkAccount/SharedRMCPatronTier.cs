// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Patrons; // Goob
using Robust.Shared.Prototypes; // Goob
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed record SharedRMCPatronTier(
    ProtoId<PatronTierPrototype> Id, // Goob - patron tier prototypes
    bool ShowOnCredits,
    bool GhostColor,
    bool GhostCosmetics, // Goob - ghost cosmetics
    bool GhostParticles, // Goob - ghost cosmetics
    bool LobbyMessage,
    bool RoundEndShoutout,
    bool OocIcons, // Goob - ooc icon selection
    string Tier,
    string? Icon
);
