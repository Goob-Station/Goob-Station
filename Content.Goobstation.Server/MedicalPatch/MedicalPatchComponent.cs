using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.MedicalPatch;

[RegisterComponent]
public sealed partial class MedicalPatchComponent : Component
{
    [DataField]
    public string SolutionName = "drink";
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(1);
    /// <summary>
    ///  if this is a single use patch, gets destroyed or replaced when empty or removed.
    /// </summary>
    [DataField]
    public bool SingleUse = false;
    /// <summary>
    ///  if single use what the Entity shud be replaced whit
    /// </summary>
    [DataField]
    public string? TrashObject = "UsedMedicalPatch";
    /// <summary>
    /// how often the patch shud transfer sulution
    /// </summary>
    [DataField]
    public float UpdateTime = 1f;

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
    /// <summary>
    /// if any set ammount shud be transfered when the patch is attatched,
    /// </summary>
    [DataField]
    public FixedPoint2 InjectAmmountOnAttatch = FixedPoint2.New(0);
    /// <summary>
    /// if a Percentage of the remaining soulution shud be transfered when attatched, use 0 - 100
    /// </summary>
    [DataField]
    public FixedPoint2 InjectPercentageOnAttatch = FixedPoint2.New(0);
}


