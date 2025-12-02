// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared.Body.Part;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;
using Content.Shared.Chemistry.Components;
namespace Content.Shared._Shitmed.Medical.HealthAnalyzer;

// Base message that contains common data for all Modes
[Serializable, NetSerializable]
public abstract class HealthAnalyzerBaseMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;
    public readonly float Temperature;
    public readonly float BloodLevel;
    public readonly bool? ScanMode;
    public readonly HealthAnalyzerMode ActiveMode;
    public Dictionary<TargetBodyPart, WoundableSeverity>? Body;
    public Dictionary<TargetBodyPart, bool> Bleeding;
    public readonly FixedPoint2 VitalDamage; // Goobstation

    public HealthAnalyzerBaseMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        HealthAnalyzerMode activeMode,
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<TargetBodyPart, bool> bleeding,
        FixedPoint2 vitalDamage)  // Goobstation
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        ActiveMode = activeMode;
        Body = body;
        Bleeding = bleeding;
        VitalDamage = vitalDamage;  // Goobstation
    }
}

// Body Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerBodyMessage : HealthAnalyzerBaseMessage
{
    public readonly bool? Unrevivable;
    public readonly NetEntity? SelectedPart;
    public readonly Dictionary<NetEntity, List<WoundableTraumaData>> Traumas;
    public readonly Dictionary<NetEntity, FixedPoint2> NervePainFeels;

    public HealthAnalyzerBodyMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        bool? unrevivable,
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<TargetBodyPart, bool> bleeding,
        FixedPoint2 vitalDamage,  // Goobstation
        Dictionary<NetEntity, List<WoundableTraumaData>> traumas,
        Dictionary<NetEntity, FixedPoint2> nervePainFeels,
        NetEntity? selectedPart = null)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Body, body, bleeding, vitalDamage)  // Goobstation
    {
        Unrevivable = unrevivable;
        SelectedPart = selectedPart;
        Traumas = traumas;
        NervePainFeels = nervePainFeels;
    }
}

// Organs Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerOrgansMessage : HealthAnalyzerBaseMessage
{
    public readonly Dictionary<NetEntity, OrganTraumaData> Organs;

    public HealthAnalyzerOrgansMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        Dictionary<TargetBodyPart, bool> bleeding,
        FixedPoint2 vitalDamage, // Goobstation
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<NetEntity, OrganTraumaData> organs)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Organs, body, bleeding, vitalDamage) // Goobstation
    {
        Organs = organs;
    }
}

// Chemicals Mode message
[Serializable, NetSerializable]
public sealed class HealthAnalyzerChemicalsMessage : HealthAnalyzerBaseMessage
{
    public readonly Dictionary<NetEntity, Solution> Solutions;

    public HealthAnalyzerChemicalsMessage(
        NetEntity? targetEntity,
        float temperature,
        float bloodLevel,
        bool? scanMode,
        Dictionary<TargetBodyPart, bool> bleeding,
        FixedPoint2 vitalDamage, // Goobstation
        Dictionary<TargetBodyPart, WoundableSeverity>? body,
        Dictionary<NetEntity, Solution> solutions)
        : base(targetEntity, temperature, bloodLevel, scanMode, HealthAnalyzerMode.Chemicals, body, bleeding, vitalDamage) // Goobstation
    {
        Solutions = solutions;
    }
}

// Mode selection message (from client to server)
[Serializable, NetSerializable]
public sealed class HealthAnalyzerModeSelectedMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? Owner;
    public readonly HealthAnalyzerMode Mode;

    public HealthAnalyzerModeSelectedMessage(NetEntity owner, HealthAnalyzerMode mode)
    {
        Owner = owner;
        Mode = mode;
    }
}

// Part selection message (from client to server)
[Serializable, NetSerializable]
public sealed class HealthAnalyzerPartSelectedMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? Owner;
    public readonly TargetBodyPart? BodyPart;

    public HealthAnalyzerPartSelectedMessage(NetEntity? owner, TargetBodyPart? bodyPart)
    {
        Owner = owner;
        BodyPart = bodyPart;
    }
}

[Serializable, NetSerializable]
public struct WoundableTraumaData
{
    public string Name;
    public string TraumaType;
    public FixedPoint2 Severity;
    public string? SeverityString; // Used mostly in Bone Damage traumas to keep track of the secondary severity.
    public (BodyPartType, BodyPartSymmetry)? TargetType;

    public WoundableTraumaData(string name,
        string traumaType,
        FixedPoint2 severity,
        string? severityString = null,
        (BodyPartType, BodyPartSymmetry)? targetType = null)
    {
        Name = name;
        TraumaType = traumaType;
        Severity = severity;
        SeverityString = severityString;
        TargetType = targetType;
    }
}

// Supporting data structures
[Serializable, NetSerializable]
public struct OrganTraumaData
{
    public FixedPoint2 Integrity;
    public FixedPoint2 IntegrityCap;
    public OrganSeverity Severity;
    public List<(string Name, FixedPoint2 Value)> Modifiers;

    public OrganTraumaData(FixedPoint2 integrity,
        FixedPoint2 integrityCap,
        OrganSeverity severity,
        List<(string Name, FixedPoint2 Value)> modifiers)
    {
        Integrity = integrity;
        IntegrityCap = integrityCap;
        Severity = severity;
        Modifiers = modifiers;
    }
}

[Serializable, NetSerializable]
public enum HealthAnalyzerMode
{
    Body,
    Organs,
    Chemicals
}
