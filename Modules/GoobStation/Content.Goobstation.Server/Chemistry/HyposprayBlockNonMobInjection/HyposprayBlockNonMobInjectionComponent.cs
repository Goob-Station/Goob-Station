// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Chemistry.HyposprayBlockNonMobInjection;

/// <summary>
/// For some reason if you set HyposprayComponent onlyAffectsMobs to true it would be able to draw from containers
/// even if injectOnly is also true. I don't want to modify HypospraySystem, so I made this component.
/// </summary>
[RegisterComponent]
public sealed partial class HyposprayBlockNonMobInjectionComponent : Component
{
}