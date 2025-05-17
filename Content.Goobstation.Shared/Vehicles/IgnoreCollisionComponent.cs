// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class IgnoreCollisionComponent : Component
{
    [DataField]
    public Dictionary<string, (int Layer, int Mask)> OriginalFixtures = new();
}
