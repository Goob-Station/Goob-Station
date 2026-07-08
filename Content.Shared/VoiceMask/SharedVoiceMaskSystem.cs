// SPDX-License-Identifier: MIT

using Content.Shared.StatusIcon; // GabyStation radio icons
using Robust.Shared.Prototypes; // GabyStation radio icons
using Robust.Shared.Serialization;

namespace Content.Shared.VoiceMask;

[Serializable, NetSerializable]
public enum VoiceMaskUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class VoiceMaskBuiState : BoundUserInterfaceState
{
    public readonly string Name;
    public readonly string? Verb;
    public ProtoId<JobIconPrototype>? JobIcon { get; } // GabyStation -> Radio icons

    public VoiceMaskBuiState(string name, string? verb, ProtoId<JobIconPrototype>? jobIcon) // GabyStation radio icons
    {
        Name = name;
        Verb = verb;
        JobIcon = jobIcon; // GabyStation -> Radio icons
    }
}

[Serializable, NetSerializable]
public sealed class VoiceMaskChangeNameMessage : BoundUserInterfaceMessage
{
    public readonly string Name;

    public VoiceMaskChangeNameMessage(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Change the speech verb prototype to override, or null to use the user's verb.
/// </summary>
[Serializable, NetSerializable]
public sealed class VoiceMaskChangeVerbMessage : BoundUserInterfaceMessage
{
    public readonly string? Verb;

    public VoiceMaskChangeVerbMessage(string? verb)
    {
        Verb = verb;
    }
}
