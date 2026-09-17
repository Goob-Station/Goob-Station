using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.LinkAccount;

[Serializable, NetSerializable]
public sealed record SharedRMCGhostCosmetics(
    string? Particles,
    string? Hat,
    string? Mask
);
