using Content.Shared.Damage;

namespace Content.Goobstation.Server.Cult.Actions;

[RegisterComponent]
public sealed partial class BloodCultActionComponent : Component
{
    /// <summary>
    ///     If the magic has been enhanced somehow, likely due to an empowering rune.
    /// </summary>
    [DataField] public bool Enhanced = false;
}
