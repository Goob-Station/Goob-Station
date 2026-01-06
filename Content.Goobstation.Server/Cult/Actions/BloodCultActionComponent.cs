namespace Content.Goobstation.Server.Cult.Actions;

[RegisterComponent]
public sealed partial class BloodCultActionComponent : Component
{
    /// <summary>
    ///     Whether it should be deleted or not after depleting charges.
    /// </summary>
    [DataField] public bool Limited = false;

    /// <summary>
    ///     How much slots in the spell space will this one take?
    /// </summary>
    [DataField] public int SlotsCost = 1;

    /// <summary>
    ///     How much health will this action drain on use.
    /// </summary>
    [DataField] public float HealthCost = 0f;

    /// <summary>
    ///     If the magic has been enhanced somehow, likely due to an empowering rune.
    /// </summary>
    [DataField] public bool Enhanced = false;

    /// <summary>
    ///    How many uses this action has left before being destroyed. Only 1 use by default.
    /// </summary>
    [DataField] public int Uses = 1;

    [DataField] public bool UnlimitedUses = false;

    [DataField] public string InvocationLoc = string.Empty;
}
