// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Devil.Actions;

public sealed partial class CreateContractEvent : InstantActionEvent;

public sealed partial class CreateRevivalContractEvent : InstantActionEvent;

public sealed partial class ShadowJauntEvent : InstantActionEvent;

public sealed partial class DevilGripEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class DevilGripCurseRotPurchasedEvent : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed partial class DevilGripEnhancedPurchasedEvent : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed partial class DevilGripCondemnPurchasedEvent : EntityEventArgs { }

public sealed partial class DevilSummonPitchforkEvent : InstantActionEvent;

public sealed partial class DevilHellstepEvent : InstantActionEvent;

public sealed partial class DevilHeresyEvent : InstantActionEvent;

public sealed partial class DevilAuthorityEvent : InstantActionEvent;

public sealed partial class DevilPossessionEvent : EntityTargetActionEvent;

public sealed partial class DevilOpenStoreEvent : InstantActionEvent;

[Serializable]
public sealed partial class BecomeLesserDevilEvent : InstantActionEvent
{
    public EntProtoId Prototype = "MobLesserDevil";
}

[Serializable]
public sealed partial class BecomeArchdevilEvent : InstantActionEvent
{
    public EntProtoId Prototype = "MobArchDevil";
}

[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class DevilFireImmuneEvent : EntityEventArgs
{
}
