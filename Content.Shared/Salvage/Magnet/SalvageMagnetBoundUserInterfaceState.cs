// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Salvage.Magnet;

[Serializable, NetSerializable]
public sealed class SalvageMagnetBoundUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan? EndTime;
    public TimeSpan NextOffer;

    public TimeSpan Cooldown;
    public TimeSpan Duration;

    public int ActiveSeed;

    public List<int> Offers;

    public SalvageMagnetBoundUserInterfaceState(List<int> offers)
    {
        Offers = offers;
    }
}