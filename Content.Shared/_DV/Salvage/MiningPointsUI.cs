using Robust.Shared.Serialization;

namespace Content.Shared._DV.Salvage;

/// <summary>
/// Message for a lathe to transfer its mining points to the user's id card.
/// </summary>
[Serializable, NetSerializable]
public sealed class LatheClaimMiningPointsMessage : BoundUserInterfaceMessage;
