// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Weapons.Ranged.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Weapons.Ranged.Components;

[RegisterComponent]
public sealed partial class AmmoCounterComponent : SharedAmmoCounterComponent
{
    public Control? Control;
}
