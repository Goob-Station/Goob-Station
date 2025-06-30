// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Components;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MisandryBox.Smites;

public sealed class RunWalkSwapSystem : ToggleableSmiteSystem<RunWalkSwapComponent>
{
    public override void Set(EntityUid owner)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(owner);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseWalkSpeed, movementSpeed.BaseSprintSpeed);
    }
}