// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Shared.Weapons.Ranged.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class AmmoCounterComponent : SharedAmmoCounterComponent
{
    public Control? Control;
}