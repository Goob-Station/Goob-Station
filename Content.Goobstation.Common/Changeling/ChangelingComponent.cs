// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Changeling;


/// <summary>
/// Exists to mark an entity as a changeling.
/// For the component holding changeling data, see ChangelingIdentityComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{

}