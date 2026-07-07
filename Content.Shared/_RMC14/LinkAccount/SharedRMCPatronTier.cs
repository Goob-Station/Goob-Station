// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed record SharedRMCPatronTier(
    bool ShowOnCredits,
    bool GhostColor,
    bool LobbyMessage,
    bool RoundEndShoutout,
    string Tier,
    string? Icon
);