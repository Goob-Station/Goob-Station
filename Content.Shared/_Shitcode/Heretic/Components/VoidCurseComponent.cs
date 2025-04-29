// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoidCurseComponent : Component
{
    [DataField, AutoNetworkedField] public float Lifetime = 10f;
    [DataField] public float MaxLifetime = 30f;
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)] public bool Drain = false;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField] public float Stacks = 1f;
    [DataField, AutoNetworkedField] public float MaxStacks = 5f;

    public float Timer = 1f;
}