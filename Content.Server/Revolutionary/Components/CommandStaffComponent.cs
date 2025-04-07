using Content.Server.GameTicking.Rules;
using Content.Server.Mindshield; // GoobStation

namespace Content.Server.Revolutionary.Components;

/// <summary>
/// Given to heads at round start. Used for assigning traitors to kill heads and for revs to check if the heads died or not.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem), typeof(MindShieldSystem))] // GoobStation - typeof MindshieldSystem
public sealed partial class CommandStaffComponent : Component
{
    // Goobstation
    /// <summary>
    /// Check for removing mindshield implant from command.
    /// </summary>
    [DataField]
    public bool Enabled = true;
}

//TODO this should probably be on a mind role, not the mob
