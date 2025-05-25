// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 brainfood1183 <113240905+brainfood1183@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Anomaly;

[Serializable, NetSerializable]
public enum AnomalyVisuals : byte
{
    IsPulsing,
    Supercritical
}

[Serializable, NetSerializable]
public enum AnomalyVisualLayers : byte
{
    Base,
    Animated
}

/// <summary>
/// The types of anomalous particles used
/// for interfacing with anomalies.
/// </summary>
/// <remarks>
/// The only thought behind these names is that
/// they're a continuation of radioactive particles.
/// Yes i know detla+ waves exist, but they're not
/// common enough for me to care.
/// </remarks>
[Serializable, NetSerializable]
public enum AnomalousParticleType : byte
{
    Delta,
    Epsilon,
    Zeta,
    Sigma,
    Default
}

[Serializable, NetSerializable]
public enum AnomalyVesselVisuals : byte
{
    HasAnomaly,
    AnomalyState
}

[Serializable, NetSerializable]
public enum AnomalyVesselVisualLayers : byte
{
    Base
}

[Serializable, NetSerializable]
public enum AnomalyGeneratorVisuals : byte
{
    Generating
}

[Serializable, NetSerializable]
public enum AnomalyGeneratorVisualLayers : byte
{
    Base
}

[Serializable, NetSerializable]
public enum AnomalyScannerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AnomalyScannerUserInterfaceState : BoundUserInterfaceState
{
    public FormattedMessage Message;

    public TimeSpan? NextPulseTime;

    public AnomalyScannerUserInterfaceState(FormattedMessage message, TimeSpan? nextPulseTime)
    {
        Message = message;
        NextPulseTime = nextPulseTime;
    }
}

[Serializable, NetSerializable]
public enum AnomalyGeneratorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AnomalyGeneratorUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan CooldownEndTime;

    public int FuelAmount;

    public int FuelCost;

    public AnomalyGeneratorUserInterfaceState(TimeSpan cooldownEndTime, int fuelAmount, int fuelCost)
    {
        CooldownEndTime = cooldownEndTime;
        FuelAmount = fuelAmount;
        FuelCost = fuelCost;
    }
}

[Serializable, NetSerializable]
public sealed class AnomalyGeneratorGenerateButtonPressedEvent : BoundUserInterfaceMessage
{

}