using Content.Shared._Shitmed.Targeting; // Shitmed Change
using Robust.Shared.Serialization;

namespace Content.Shared.MedicalScanner;

/// <summary>
///     On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerScannedUserMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public Dictionary<TargetBodyPart, TargetIntegrity>? Body; // Shitmed Change
    public NetEntity? Part; // Shitmed Change

    public HealthAnalyzerScannedUserMessage(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, Dictionary<TargetBodyPart, TargetIntegrity>? body, NetEntity? part = null) // Shitmed Change
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Body = body; // Shitmed Change
        Part = part; // Shitmed Change
    }
}

// Shitmed Change Start
[Serializable, NetSerializable]
public sealed class HealthAnalyzerPartMessage(NetEntity? owner, TargetBodyPart? bodyPart) : BoundUserInterfaceMessage
{
    public readonly NetEntity? Owner = owner;
    public readonly TargetBodyPart? BodyPart = bodyPart;

}
// Shitmed Change End
