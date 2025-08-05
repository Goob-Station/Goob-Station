// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Devil.Actions;

public sealed partial class CreateContractEvent : InstantActionEvent;

public sealed partial class CreateRevivalContractEvent : InstantActionEvent;

public sealed partial class ShadowJauntEvent : InstantActionEvent;

public sealed partial class DevilGripEvent : InstantActionEvent;

public sealed partial class DevilPossessionEvent : EntityTargetActionEvent;
