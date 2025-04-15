// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Changeling.GameTicking.Rules;
using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.GameTicking.Rules;

[RegisterComponent, Access(typeof(DevilRuleSystem))]
public sealed partial class DevilRuleComponent : Component
{
}
