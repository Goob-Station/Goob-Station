using Content.Server.GameTicking.Rules;
using Content.Server.Mindshield;

namespace Content.Server.Revolutionary.Components;

/// <summary>
/// Given to heads at round start for Revs. Used for tracking if heads died or not.
/// </summary>
[RegisterComponent, Access(typeof(RevolutionaryRuleSystem), typeof(MindShieldSystem))]
public sealed partial class CommandStaffComponent : Component
{
    /// <summary>
    /// Check for removing mindshield implant from command.
    /// </summary>
    [DataField]
    public bool Enabled = true; // Goobstation
}
