// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Devil.Contract;
using Content.Shared.Inventory;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Devil;

/// <summary>
/// Raised on a devil when their power level changes.
/// </summary>
/// <param name="User">The Devil whos power level is changing</param>
/// <param name="NewLevel">The new level they are reaching.</param>
[ByRefEvent]
public record struct PowerLevelChangedEvent(EntityUid User, DevilPowerLevel NewLevel);

/// <summary>
/// Raised on a devil when the amount of souls in their storage changes.
/// </summary>
/// <param name="User">The Devil gaining souls.</param>
/// <param name="Victim">The entity losing its soul.</param>
/// <param name="Amount">How many souls they are gaining.</param>
[ByRefEvent]
public record struct SoulAmountChangedEvent(EntityUid User, EntityUid Victim, int Amount);

/// <summary>
/// Raised on an entity to see if their eyes are covered.
/// This just checks for the identity blocker comp.
/// </summary>
/// <param name="Target"></param>
public sealed class IsEyesCoveredCheckEvent : EntityEventArgs, IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.MASK | SlotFlags.HEAD;

    public bool IsEyesProtected;
}

// Contract Events

[ImplicitDataDefinitionForInheritors, DataDefinition]
public abstract partial class BaseDevilContractEvent : EntityEventArgs
{
    /// <summary>
    /// The contract using this event.
    /// </summary>
    public DevilContractComponent? Contract;

    /// <summary>
    /// The target affected by this contract.
    /// </summary>
    public EntityUid Target;
}

[DataDefinition, Serializable]
public sealed partial class DevilContractSoulOwnershipEvent : BaseDevilContractEvent;

[DataDefinition, Serializable]
public sealed partial class DevilContractLoseHandEvent : BaseDevilContractEvent;

[DataDefinition, Serializable]
public sealed partial class DevilContractLoseLegEvent : BaseDevilContractEvent;

[DataDefinition, Serializable]
public sealed partial class DevilContractLoseOrganEvent : BaseDevilContractEvent;

[DataDefinition, Serializable]
public sealed partial class DevilContractChanceEvent : BaseDevilContractEvent;
