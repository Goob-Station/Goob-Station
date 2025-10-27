// SPDX-FileCopyrightText: 2019 Injazz <43905364+Injazz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 ScalyChimp <72841710+scaly-chimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Trigger
{
    [Serializable, NetSerializable]
    public enum ProximityTriggerVisuals : byte
    {
        Off,
        Inactive,
        Active,
    }

    [Serializable, NetSerializable]
    public enum ProximityTriggerVisualState : byte
    {
        State,
    }

    [Serializable, NetSerializable]
    public enum TriggerVisuals : byte
    {
        VisualState,
    }

    [Serializable, NetSerializable]
    public enum TriggerVisualState : byte
    {
        Primed,
        Unprimed,
    }
}