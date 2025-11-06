
using Content.Shared.Store;
using Robust.Shared.Configuration;

namespace Content.Server.Store.Conditions;

/// <summary>
/// Only allows a listing based on bool CVAR
/// </summary>
public sealed partial class CcvarListingCondition : ListingCondition
{
    /// <summary>
    /// name of cvar
    /// </summary>
    [DataField( required: true)]
    public string Name;

    /// <summary>
    /// inverts showing the listing
    /// </summary>
    [DataField]
    public bool Invert = true;

    /// <summary>
    /// shud this item be listed if cvar dont exist
    /// </summary>
    [DataField]
    public bool ReturnIfFail = false;

    [Dependency] private readonly IConfigurationManager CfgManager = default!;
    public override bool Condition(ListingConditionArgs args)
    {
        if (!CfgManager.IsCVarRegistered(Name))
            return ReturnIfFail; // cvar dont exist

        if (Invert)
            return !CfgManager.GetCVar<bool>(Name);

        return CfgManager.GetCVar<bool>(Name);
    }
}
